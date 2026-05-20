using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Internal.Packs;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests;

/// <summary>
/// §6.1 disposal-contract exit-state matrix — one Fact per row. Verifies the BuildAsync
/// partial-build disposal contract: reverse-order dispose, async-first, no double-dispose
/// of transient, cooperative CT checks at Phase 3→4 and Phase 4→5 boundaries, AggregateException
/// preservation of the original exception, _isAlreadyBuilt only set on success path.
/// </summary>
[Collection(nameof(DisposalContractTests))]
[CollectionDefinition(nameof(DisposalContractTests), DisableParallelization = true)]
public class DisposalContractTests
{
    [Fact]
    public async Task BuildAsync_AllPacksOk_ReturnsFinalProvider()
    {
        var transientHook = new DisposeHook();
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack()
                .Configure(c => c.Add<TransientHook>().AsSelf().Singleton())
                .Register(
                    (_, p) =>
                    {
                        p.GetRequiredService<TransientHook>().Hook = transientHook;
                    }
                )
        );

        await using var final = await builder.BuildAsync(TestContext.Current.CancellationToken);

        // transient was disposed at Phase 4 step 7 → its TransientHook was disposed
        transientHook.DisposedCount.Is(1);
    }

    [Fact]
    public async Task BuildAsync_PackThrowsInConfigure_DisposesNothing()
    {
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack().Configure((_, _) => throw new InvalidOperationException("configure-boom"))
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await builder.BuildAsync(TestContext.Current.CancellationToken)
        );

        ex.Message.IsEqual("configure-boom");
    }

    [Fact]
    public async Task BuildAsync_PackThrowsInRegister_DisposesTransient()
    {
        var transientHook = new DisposeHook();
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack()
                .Configure(c => c.Add<TransientHook>().AsSelf().Singleton())
                .Register(
                    (_, p) =>
                    {
                        p.GetRequiredService<TransientHook>().Hook = transientHook;
                        throw new InvalidOperationException("register-boom");
                    }
                )
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await builder.BuildAsync(TestContext.Current.CancellationToken)
        );

        ex.Message.IsEqual("register-boom");
        transientHook.DisposedCount.Is(1);
    }

    [Fact]
    public async Task BuildAsync_PackThrowsInSetup_DisposesFinal_NotTransient()
    {
        var transientHook = new DisposeHook();
        var finalHook = new DisposeHook();
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack()
                .Configure(c =>
                {
                    c.Add<TransientHook>().AsSelf().Singleton();
                    c.Add<FinalHook>().AsSelf().Singleton();
                })
                .Register(
                    (_, p) =>
                    {
                        p.GetRequiredService<TransientHook>().Hook = transientHook;
                    }
                )
                .Setup(
                    p =>
                    {
                        p.GetRequiredService<FinalHook>().Hook = finalHook;
                        throw new InvalidOperationException("setup-boom");
                    }
                )
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await builder.BuildAsync(TestContext.Current.CancellationToken)
        );

        ex.Message.IsEqual("setup-boom");
        // catch handler disposes final → finalHook.Dispose ran once
        finalHook.DisposedCount.Is(1);
        // transient was disposed at Phase 4 step 7 AND nulled — catch handler skipped re-dispose
        transientHook.DisposedCount.Is(1);
    }

    [Fact]
    public async Task BuildAsync_CancelledDuringPhase1_ThrowsOCE_DisposesNothing()
    {
        var cts = new CancellationTokenSource();
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack().Configure(async (_, ct) => await Task.Delay(Timeout.Infinite, ct))
        );

        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(
            async () => await builder.BuildAsync(cts.Token)
        );
        ex.CancellationToken.Is(cts.Token);
    }

    [Fact]
    public async Task BuildAsync_CancelledDuringPhase3_ThrowsOCE_DisposesTransient()
    {
        var cts = new CancellationTokenSource();
        var transientHook = new DisposeHook();
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack()
                .Configure(c => c.Add<TransientHook>().AsSelf().Singleton())
                .Register(
                    async (_, p, ct) =>
                    {
                        p.GetRequiredService<TransientHook>().Hook = transientHook;
                        await Task.Delay(Timeout.Infinite, ct);
                    }
                )
        );

        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(
            async () => await builder.BuildAsync(cts.Token)
        );
        ex.CancellationToken.Is(cts.Token);
        transientHook.DisposedCount.Is(1);
    }

    [Fact]
    public async Task BuildAsync_CancelledBetweenPhase3AndPhase4_BoundaryCheckThrows_DisposesTransient_FinalNeverBuilt()
    {
        var cts = new CancellationTokenSource();
        var transientHook = new DisposeHook();
        var finalHook = new DisposeHook();
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack()
                .Configure(c =>
                {
                    c.Add<TransientHook>().AsSelf().Singleton();
                    c.Add<FinalHook>().AsSelf().Singleton();
                })
                .Register(
                    async (_, p, _) =>
                    {
                        p.GetRequiredService<TransientHook>().Hook = transientHook;
                        // Phase 3 returns normally; cancel before BuildAsync reaches the Phase 3→4 boundary check (step 6)
                        await cts.CancelAsync();
                    }
                )
                .Setup(
                    p =>
                    {
                        // would only run if Phase 5 reached
                        p.GetRequiredService<FinalHook>().Hook = finalHook;
                    }
                )
        );

        var ex = await Assert.ThrowsAsync<OperationCanceledException>(async () => await builder.BuildAsync(cts.Token));
        ex.CancellationToken.Is(cts.Token);
        transientHook.DisposedCount.Is(1);
        finalHook.DisposedCount.Is(0); // FinalHook never materialised → never disposed
    }

    [Fact]
    public async Task BuildAsync_CancelledBetweenPhase4AndPhase5_BoundaryCheckThrows_DisposesFinal_NoDoubleDisposeOfTransient()
    {
        var cts = new CancellationTokenSource();
        var transientHook = new DisposeHook();
        var setupRan = false;
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack()
                .Configure(c =>
                {
                    c.Add<TransientHook>().AsSelf().Singleton();
                    c.Add<TransientCanceller>().AsSelf().Singleton();
                })
                .Register(
                    (_, p) =>
                    {
                        p.GetRequiredService<TransientHook>().Hook = transientHook;
                        // Materialise the canceller in transient: when transient.Dispose() runs at
                        // Phase 4 step 7, the canceller fires cts.Cancel(). Then Phase 4→5 boundary
                        // check at step 8 throws OCE.
                        p.GetRequiredService<TransientCanceller>().Cts = cts;
                    }
                )
                .Setup(
                    _ =>
                    {
                        setupRan = true;
                    }
                )
        );

        var ex = await Assert.ThrowsAsync<OperationCanceledException>(async () => await builder.BuildAsync(cts.Token));
        ex.CancellationToken.Is(cts.Token);
        // transient disposed exactly once at Phase 4 step 7; catch handler does NOT re-dispose it (nulled)
        transientHook.DisposedCount.Is(1);
        setupRan.IsFalse();
    }

    [Fact]
    public async Task BuildAsync_CancelledDuringPhase5_ThrowsOCE_DisposesFinal_NoDoubleDisposeOfTransient()
    {
        var cts = new CancellationTokenSource();
        var transientHook = new DisposeHook();
        var finalHook = new DisposeHook();
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack()
                .Configure(c =>
                {
                    c.Add<TransientHook>().AsSelf().Singleton();
                    c.Add<FinalHook>().AsSelf().Singleton();
                })
                .Register(
                    (_, p) =>
                    {
                        p.GetRequiredService<TransientHook>().Hook = transientHook;
                    }
                )
                .Setup(
                    async (p, ct) =>
                    {
                        p.GetRequiredService<FinalHook>().Hook = finalHook;
                        await cts.CancelAsync();
                        await Task.Delay(Timeout.Infinite, ct);
                    }
                )
        );

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(async () => await builder.BuildAsync(cts.Token));
        ex.CancellationToken.Is(cts.Token);
        // catch handler disposes final → finalHook materialised → finalHook.Dispose ran once
        finalHook.DisposedCount.Is(1);
        // transient was disposed at Phase 4 step 7 and nulled — catch handler skipped re-dispose
        transientHook.DisposedCount.Is(1);
    }

    [Fact]
    public async Task BuildAsync_DisposeAsyncThrowsAfterPhase5Failure_AggregatesOriginalAndDisposeError()
    {
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack()
                .Configure(c => c.Add<ThrowOnAsyncDispose>().AsSelf().Singleton())
                .Setup(
                    p =>
                    {
                        // materialise so SP tracks it for dispose
                        _ = p.GetRequiredService<ThrowOnAsyncDispose>();
                        throw new InvalidOperationException("phase5-boom");
                    }
                )
        );

        var ex = await Assert.ThrowsAsync<AggregateException>(
            async () => await builder.BuildAsync(TestContext.Current.CancellationToken)
        );

        // InnerExceptions[0] is the original phase-5 exception
        ex.InnerExceptions[0].Message.IsEqual("phase5-boom");
        // InnerExceptions[1..] are dispose errors — M.E.DI's SP.DisposeAsync wraps the throwing
        // singleton in its own AggregateException, but the dispose error itself is recorded here.
        ex.InnerExceptions.Count.Is(2);
    }

    [Fact]
    public async Task BuildAsync_FailedRunDoesNotSetIsAlreadyBuilt()
    {
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack().Setup(_ => throw new InvalidOperationException("setup-boom"))
        );

        // first build fails in Phase 5
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await builder.BuildAsync(TestContext.Current.CancellationToken)
        );

        // second build on SAME builder should reproduce the same fault, NOT throw "already built"
        var secondEx = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await builder.BuildAsync(TestContext.Current.CancellationToken)
        );
        secondEx.Message.IsEqual("setup-boom");
    }

    /// <summary>
    /// Regression guard for §8.2.1 step 7 nulling. Tests
    /// <see cref="ServiceProviderBuilder.DisposeWithAggregationAsync"/> directly with both providers
    /// non-null and verifies the dispose order is final-then-transient. If a future refactor
    /// omits the `transient = null;` line, the catch handler could find both providers alive at
    /// the same time — this test asserts the order is preserved in that case.
    /// </summary>
    [Fact]
    public async Task BuildAsync_Phase4NullsTransientAfterDispose_NoDoubleDisposeRegression()
    {
        var order = new List<string>();
        var finalSp = BuildSpWithOrderedHook("final", order);
        var transientSp = BuildSpWithOrderedHook("transient", order);

        var original = new InvalidOperationException("original");

        await ServiceProviderBuilder.DisposeWithAggregationAsync(original, finalSp, transientSp);

        // dispose order: final before transient
        order.Count.Is(2);
        order[0].IsEqual("final");
        order[1].IsEqual("transient");
    }

    /// <summary>
    /// Verifies depth-first walker ordering across nested <see cref="ServicePackBase"/> trees:
    /// each phase iterates child packs (depth-first) before invoking the parent's hook.
    /// Guards <see cref="ServicePackBase.InternalConfigureAsync"/>, <see cref="ServicePackBase.InternalRegisterAsync"/>,
    /// and <see cref="ServicePackBase.InternalSetupAsync"/> against a regression that calls the parent
    /// hook before walking children.
    /// </summary>
    [Fact]
    public async Task BuildAsync_NestedServicePacks_DepthFirstChildBeforeParent()
    {
        var order = new List<string>();
        WalkOrderTracker.Sink = order;

        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack<ParentPack>();

        await using var sp = await builder.BuildAsync(TestContext.Current.CancellationToken);

        // Expect: each phase runs child before parent.
        order.Count.Is(6);
        order[0].IsEqual("child:configure");
        order[1].IsEqual("parent:configure");
        order[2].IsEqual("child:register");
        order[3].IsEqual("parent:register");
        order[4].IsEqual("child:setup");
        order[5].IsEqual("parent:setup");

        WalkOrderTracker.Sink = null;
    }

    /// <summary>
    /// Verifies that the M.E.DI host bridge — <see cref="ServiceProviderFactory.CreateServiceProvider"/> —
    /// still returns a working <see cref="IServiceProvider"/> synchronously. This preserves the
    /// ASP.NET Core / Blazor call shape used via <c>UseServiceProviderFactory</c>.
    /// </summary>
    [Fact]
    public void CreateServiceProvider_MEDIBridge_ReturnsWorkingProvider()
    {
        var factory = new ServiceProviderFactory();
        var builder = factory.CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack().Configure(c => c.Add<TransientHook>().AsSelf().Singleton())
        );

        var sp = factory.CreateServiceProvider(builder);

        // GetRequiredService throws InvalidOperationException if the service can't be resolved,
        // so a successful resolution + non-null instance proves the provider is operational.
        var hook = sp.GetRequiredService<TransientHook>();
        hook.IsNotDefault();
    }

    private static ServiceProvider BuildSpWithOrderedHook(string tag, List<string> order)
    {
        var services = new ServiceCollection();
        // factory form so SP takes ownership of the disposable (instance-overload doesn't track)
        services.AddSingleton<OrderedDisposeHook>(_ => new OrderedDisposeHook(tag, order));
        var sp = services.BuildServiceProvider();
        // materialise so SP tracks for dispose
        _ = sp.GetRequiredService<OrderedDisposeHook>();
        return sp;
    }
}

/// <summary>
/// Test singleton tracked by transient SP. Holds a reference to a test-scoped <see cref="DisposeHook"/>
/// the test injects post-resolve so SP.Dispose triggers the hook's Dispose counter.
/// </summary>
internal sealed class TransientHook : IDisposable
{
    public DisposeHook? Hook;

    public void Dispose() => Hook?.Dispose();
}

/// <summary>
/// Test singleton tracked by final SP. Mirrors <see cref="TransientHook"/> but materialised in final
/// (resolved during Setup).
/// </summary>
internal sealed class FinalHook : IDisposable
{
    public DisposeHook? Hook;

    public void Dispose() => Hook?.Dispose();
}

/// <summary>
/// Singleton helper that counts how many times its <see cref="Dispose"/> method has been invoked.
/// Test-scoped (constructed per Fact) so static state cannot bleed between tests.
/// </summary>
internal sealed class DisposeHook
{
    public int DisposedCount;

    public void Dispose() => Interlocked.Increment(ref DisposedCount);
}

/// <summary>
/// Hook that records the order of dispose calls into a shared list. Used by the
/// final-then-transient regression guard (#13).
/// </summary>
internal sealed class OrderedDisposeHook(string tag, List<string> sink) : IDisposable
{
    public void Dispose()
    {
        lock (sink)
            sink.Add(tag);
    }
}

/// <summary>
/// Cancels a test-scoped <see cref="CancellationTokenSource"/> when disposed — used to drive the
/// Phase 4 → Phase 5 boundary check from inside transient.Dispose() at step 7.
/// </summary>
internal sealed class TransientCanceller : IDisposable
{
    public CancellationTokenSource? Cts;

#pragma warning disable VSTHRD103
    public void Dispose() => Cts?.Cancel();
#pragma warning restore VSTHRD103
}

/// <summary>
/// Singleton IAsyncDisposable that throws from <see cref="DisposeAsync"/>. Used to drive
/// the dispose-error aggregation path in <see cref="ServiceProviderBuilder.DisposeWithAggregationAsync"/>.
/// </summary>
internal sealed class ThrowOnAsyncDispose : IAsyncDisposable
{
    public ValueTask DisposeAsync() => throw new InvalidOperationException("dispose-boom");
}

/// <summary>
/// Test-scoped sink for depth-first walker ordering verification. Per-fact: tests set the Sink
/// before constructing the pack tree and null it out at end. Collection-level parallelization is
/// disabled on <see cref="DisposalContractTests"/> so static state is safe.
/// </summary>
internal static class WalkOrderTracker
{
    public static List<string>? Sink;

    public static void Record(string entry)
    {
        if (Sink is null)
            return;
        lock (Sink)
            Sink.Add(entry);
    }
}

/// <summary>
/// Leaf pack: records every phase invocation in <see cref="WalkOrderTracker"/>.
/// </summary>
internal sealed class ChildPack : ServicePackBase
{
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        WalkOrderTracker.Record("child:configure");
        return Task.CompletedTask;
    }

    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        WalkOrderTracker.Record("child:register");
        return Task.CompletedTask;
    }

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        WalkOrderTracker.Record("child:setup");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Parent pack: nests <see cref="ChildPack"/> via <see cref="ServicePackBase.Add{T}"/> and records
/// its own phase invocations.
/// </summary>
internal sealed class ParentPack : ServicePackBase
{
    public ParentPack() => Add<ChildPack>();

    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        WalkOrderTracker.Record("parent:configure");
        return Task.CompletedTask;
    }

    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        WalkOrderTracker.Record("parent:register");
        return Task.CompletedTask;
    }

    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        WalkOrderTracker.Record("parent:setup");
        return Task.CompletedTask;
    }
}
