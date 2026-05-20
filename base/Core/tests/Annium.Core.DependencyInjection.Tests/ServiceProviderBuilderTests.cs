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
    [Fact]
    public async Task Build_SecondCall_Throws()
    {
        // arrange
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        await using var provider = await builder.BuildAsync(TestContext.Current.CancellationToken);

        // act + assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await builder.BuildAsync(TestContext.Current.CancellationToken)
        );
        ex.Message.IsEqual("ServiceProviderBuilder is already built");
    }

    /// <summary>
    /// Verifies that a Configure-phase throw leaves the builder in its pre-build state —
    /// the "already built" flag is not flipped prematurely, so re-calling BuildAsync produces
    /// the same underlying fault rather than a misleading "already built" error.
    /// </summary>
    [Fact]
    public async Task Build_WhenConfigureThrows_PreservesRetryability()
    {
        // arrange
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack<ThrowingConfigurePack>();

        // first Build propagates the Configure fault
        var firstEx = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await builder.BuildAsync(TestContext.Current.CancellationToken)
        );
        firstEx.Message.IsEqual("boom");

        // act — second Build on same builder reproduces the underlying fault,
        // NOT "already built" — proves _isAlreadyBuilt was not flipped on throw
        var secondEx = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await builder.BuildAsync(TestContext.Current.CancellationToken)
        );

        // assert
        secondEx.Message.IsEqual("boom");
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
    /// <inheritdoc/>
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        container.Add<DisposableProbe>().AsSelf().Singleton();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
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
    /// <inheritdoc/>
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct) =>
        throw new InvalidOperationException("boom");
}
