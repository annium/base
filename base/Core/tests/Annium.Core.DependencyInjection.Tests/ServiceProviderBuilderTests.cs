using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests;

/// <summary>
/// Regression tests for <see cref="ServiceProviderFactory"/> / <see cref="IServiceProviderBuilder"/>
/// covering the three-phase build lifecycle: transient-provider disposal, fault isolation on
/// Configure-throw, and single-use idempotency.
/// </summary>
public class ServiceProviderBuilderTests
{
    /// <summary>
    /// Verifies that the transient provider built between Configure and Register is disposed
    /// after the final provider is built, so any singletons it materialized are released.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Build_TransientProvider_IsDisposed()
    {
        // arrange
        DisposableProbe.Reset();
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack<ProbePack>();

        // act
        await using var provider = await builder.BuildAsync(TestContext.Current.CancellationToken);

        // assert — the transient provider disposed its materialized probe instance;
        // the final provider holds its own probe instance (still alive, not yet disposed)
        DisposableProbe.DisposedCount.Is(1);
    }

    /// <summary>
    /// Verifies that a second call to BuildAsync on the same builder throws with a clear
    /// "already built" message, preserving the single-use contract.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Build_SecondCall_Throws()
    {
        // arrange
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        await using var provider = await builder.BuildAsync(TestContext.Current.CancellationToken);

        // act + assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await builder.BuildAsync(TestContext.Current.CancellationToken)
        );
        ex.Message.IsEqual("ServiceProviderBuilder is already built");
    }

    /// <summary>
    /// Verifies that a Configure-phase throw leaves the builder in its pre-build state —
    /// the "already built" flag is not flipped prematurely, so re-calling BuildAsync produces
    /// the same underlying fault rather than a misleading "already built" error.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Build_WhenConfigureThrows_PreservesRetryability()
    {
        // arrange
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack<ThrowingConfigurePack>();

        // first Build propagates the Configure fault
        var firstEx = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await builder.BuildAsync(TestContext.Current.CancellationToken)
        );
        firstEx.Message.IsEqual("boom");

        // act — second Build on same builder reproduces the underlying fault,
        // NOT "already built" — proves _isAlreadyBuilt was not flipped on throw
        var secondEx = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await builder.BuildAsync(TestContext.Current.CancellationToken)
        );

        // assert
        secondEx.Message.IsEqual("boom");
    }

    /// <summary>
    /// Verifies that the generic UseServicePack overload deduplicates by type so a second call with
    /// the same pack type is a no-op — Configure runs exactly once.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task UseServicePack_GenericDedupesByType()
    {
        // arrange
        CountingPack.Reset();
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack<CountingPack>();
        builder.UseServicePack<CountingPack>();

        // act
        await using var provider = await builder.BuildAsync(TestContext.Current.CancellationToken);

        // assert — only one Configure invocation despite two UseServicePack calls
        CountingPack.ConfigureCount.Is(1);
    }

    /// <summary>
    /// Verifies that the instance overload of UseServicePack allows two distinct instances of
    /// the same type — both Configure callbacks run.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task UseServicePack_InstanceAllowsTwoOfSameType()
    {
        // arrange
        CountingPack.Reset();
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(new CountingPack());
        builder.UseServicePack(new CountingPack());

        // act
        await using var provider = await builder.BuildAsync(TestContext.Current.CancellationToken);

        // assert — two distinct instances → two Configure invocations
        CountingPack.ConfigureCount.Is(2);
    }

    /// <summary>
    /// Verifies that passing the same instance twice to the instance overload of UseServicePack is a
    /// no-op for the second call — Configure runs exactly once.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task UseServicePack_InstanceDedupesByReference()
    {
        // arrange
        CountingPack.Reset();
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        var pack = new CountingPack();
        builder.UseServicePack(pack);
        builder.UseServicePack(pack);

        // act
        await using var provider = await builder.BuildAsync(TestContext.Current.CancellationToken);

        // assert — same reference → deduplicated → one Configure invocation
        CountingPack.ConfigureCount.Is(1);
    }

    /// <summary>
    /// Verifies that the ServiceProviderFactory constructor that accepts a configure action
    /// invokes the action when CreateBuilder is called.
    /// </summary>
    [Fact]
    public void ServiceProviderFactory_WithConfigureAction_InvokesConfigure()
    {
        var configured = false;
        var factory = new ServiceProviderFactory(builder =>
        {
            configured = true;
        });
        factory.CreateBuilder(new ServiceCollection());
        configured.IsTrue();
    }
}

/// <summary>
/// Probe that tracks how many times its Dispose method is invoked across service-provider
/// lifecycles. Used to observe transient-provider cleanup from outside the builder.
/// </summary>
internal sealed class DisposableProbe : IDisposable
{
    /// <summary>
    /// Cumulative dispose count across all probe instances since the last Reset.
    /// </summary>
    public static int DisposedCount;

    /// <summary>
    /// Resets the dispose counter to zero.
    /// </summary>
    public static void Reset() => Interlocked.Exchange(ref DisposedCount, 0);

    /// <summary>
    /// Disposes the probe, incrementing the shared counter atomically.
    /// </summary>
    public void Dispose() => Interlocked.Increment(ref DisposedCount);
}

/// <summary>
/// Test service pack that resolves a <see cref="DisposableProbe"/> singleton during the Register
/// phase, forcing the transient provider to materialize the instance so it can be observed on
/// disposal.
/// </summary>
internal sealed class ProbePack : ServicePackBase
{
    /// <summary>
    /// Registers <see cref="DisposableProbe"/> as a singleton so it can be materialized during the
    /// Register phase and later observed when the transient provider is disposed.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <param name="ct">Cancellation token for the configuration phase.</param>
    /// <returns>A task that represents the asynchronous configure operation.</returns>
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        container.Add<DisposableProbe>().AsSelf().Singleton();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Resolves <see cref="DisposableProbe"/> from the transient provider so the provider
    /// materializes and owns the singleton instance, making it observable on disposal.
    /// </summary>
    /// <param name="container">The service container (not used in this pack).</param>
    /// <param name="provider">The transient provider used to resolve the probe.</param>
    /// <param name="ct">Cancellation token for the register phase.</param>
    /// <returns>A task that represents the asynchronous register operation.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        // resolve from the transient provider so it materializes and owns a singleton instance
        _ = provider.Resolve<DisposableProbe>();
        return Task.CompletedTask;
    }
}

/// <summary>
/// Test service pack that deliberately throws from its ConfigureAsync method, used to verify
/// that builder state survives a mid-phase fault.
/// </summary>
internal sealed class ThrowingConfigurePack : ServicePackBase
{
    /// <summary>
    /// Always throws <see cref="InvalidOperationException"/> to simulate a Configure-phase fault,
    /// verifying that the builder's built flag is not set prematurely.
    /// </summary>
    /// <param name="container">The service container (not used).</param>
    /// <param name="ct">Cancellation token for the configuration phase.</param>
    /// <returns>Never returns normally — always throws.</returns>
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct) =>
        throw new InvalidOperationException("boom");
}

/// <summary>
/// Test service pack that increments a static counter on Configure, used to verify
/// deduplication semantics of <see cref="IServiceProviderBuilder.UseServicePack{T}"/> and
/// <see cref="IServiceProviderBuilder.UseServicePack(ServicePackBase)"/>.
/// </summary>
internal sealed class CountingPack : ServicePackBase
{
    /// <summary>Tracks how many times ConfigureAsync ran across all instances since last Reset.</summary>
    public static int ConfigureCount;

    /// <summary>Resets the shared counter to zero.</summary>
    public static void Reset() => Interlocked.Exchange(ref ConfigureCount, 0);

    /// <summary>Increments <see cref="ConfigureCount"/> when configure runs.</summary>
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        Interlocked.Increment(ref ConfigureCount);
        return Task.CompletedTask;
    }
}
