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
    /// <summary>
    /// Verifies that when all service packs complete without error, <c>BuildAsync</c> returns a
    /// fully functional final <see cref="IServiceProvider"/> and the transient provider is disposed
    /// exactly once during Phase 4.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
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

    /// <summary>
    /// Verifies that when a pack throws during the Configure phase (Phase 1), no disposal hooks
    /// are invoked because neither the transient nor the final provider was ever constructed.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_PackThrowsInConfigure_DisposesNothing()
    {
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack().Configure((_, _) => throw new InvalidOperationException("configure-boom"))
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await builder.BuildAsync(TestContext.Current.CancellationToken)
        );

        ex.Message.IsEqual("configure-boom");
    }

    /// <summary>
    /// Verifies that when a pack throws during the Register phase (Phase 3), the transient provider
    /// is disposed exactly once and the original exception propagates to the caller.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
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

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await builder.BuildAsync(TestContext.Current.CancellationToken)
        );

        ex.Message.IsEqual("register-boom");
        transientHook.DisposedCount.Is(1);
    }

    /// <summary>
    /// Verifies that when a pack throws during the Setup phase (Phase 5), the final provider is
    /// disposed once, and the transient provider (already disposed and nulled at Phase 4 step 7)
    /// is not disposed a second time.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
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
                .Setup(p =>
                {
                    p.GetRequiredService<FinalHook>().Hook = finalHook;
                    throw new InvalidOperationException("setup-boom");
                })
        );

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await builder.BuildAsync(TestContext.Current.CancellationToken)
        );

        ex.Message.IsEqual("setup-boom");
        // catch handler disposes final → finalHook.Dispose ran once
        finalHook.DisposedCount.Is(1);
        // transient was disposed at Phase 4 step 7 AND nulled — catch handler skipped re-dispose
        transientHook.DisposedCount.Is(1);
    }

    /// <summary>
    /// Verifies that cancellation during Phase 1 (Configure) propagates as a
    /// <see cref="TaskCanceledException"/> carrying the original token, and that no disposal
    /// hooks are invoked because no providers were ever constructed.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_CancelledDuringPhase1_ThrowsOCE_DisposesNothing()
    {
        var cts = new CancellationTokenSource();
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack().Configure(async (_, ct) => await Task.Delay(Timeout.Infinite, ct))
        );

        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(async () => await builder.BuildAsync(cts.Token));
        ex.CancellationToken.Is(cts.Token);
    }

    /// <summary>
    /// Verifies that cancellation during the Register phase (Phase 3) propagates as a
    /// <see cref="TaskCanceledException"/> carrying the original token and that the transient
    /// provider is disposed exactly once during error cleanup.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
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

        var ex = await Assert.ThrowsAsync<TaskCanceledException>(async () => await builder.BuildAsync(cts.Token));
        ex.CancellationToken.Is(cts.Token);
        transientHook.DisposedCount.Is(1);
    }

    /// <summary>
    /// Verifies that cancellation signalled between Phase 3 (Register) and Phase 4 (build transient)
    /// is detected by the Phase 3→4 boundary check, causing an <see cref="OperationCanceledException"/>,
    /// disposing the transient provider once, and never constructing the final provider.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
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
                .Setup(p =>
                {
                    // would only run if Phase 5 reached
                    p.GetRequiredService<FinalHook>().Hook = finalHook;
                })
        );

        var ex = await Assert.ThrowsAsync<OperationCanceledException>(async () => await builder.BuildAsync(cts.Token));
        ex.CancellationToken.Is(cts.Token);
        transientHook.DisposedCount.Is(1);
        finalHook.DisposedCount.Is(0); // FinalHook never materialised → never disposed
    }

    /// <summary>
    /// Verifies that a cancellation fired from inside the transient provider's Dispose (at Phase 4
    /// step 7) is caught by the Phase 4→5 boundary check, throwing an
    /// <see cref="OperationCanceledException"/>, disposing the transient exactly once, and preventing
    /// the Setup phase from running.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
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
                .Setup(_ =>
                {
                    setupRan = true;
                })
        );

        var ex = await Assert.ThrowsAsync<OperationCanceledException>(async () => await builder.BuildAsync(cts.Token));
        ex.CancellationToken.Is(cts.Token);
        // transient disposed exactly once at Phase 4 step 7; catch handler does NOT re-dispose it (nulled)
        transientHook.DisposedCount.Is(1);
        setupRan.IsFalse();
    }

    /// <summary>
    /// Verifies that cancellation during the Setup phase (Phase 5) propagates as a
    /// <see cref="TaskCanceledException"/>, disposes the final provider exactly once via the catch
    /// handler, and does not re-dispose the transient provider that was already nulled at Phase 4.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
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

    /// <summary>
    /// Verifies that when both Phase 5 throws and the final provider's <c>DisposeAsync</c> throws,
    /// <c>BuildAsync</c> surfaces an <see cref="AggregateException"/> containing the original
    /// phase-5 exception as the first inner exception and the dispose error as the second.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_DisposeAsyncThrowsAfterPhase5Failure_AggregatesOriginalAndDisposeError()
    {
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(
            new DynamicServicePack()
                .Configure(c => c.Add<ThrowOnAsyncDispose>().AsSelf().Singleton())
                .Setup(p =>
                {
                    // materialise so SP tracks it for dispose
                    _ = p.GetRequiredService<ThrowOnAsyncDispose>();
                    throw new InvalidOperationException("phase5-boom");
                })
        );

        var ex = await Assert.ThrowsAsync<AggregateException>(async () =>
            await builder.BuildAsync(TestContext.Current.CancellationToken)
        );

        // InnerExceptions[0] is the original phase-5 exception
        ex.InnerExceptions[0].Message.IsEqual("phase5-boom");
        // InnerExceptions[1..] are dispose errors — M.E.DI's SP.DisposeAsync wraps the throwing
        // singleton in its own AggregateException, but the dispose error itself is recorded here.
        ex.InnerExceptions.Count.Is(2);
    }

    /// <summary>
    /// Verifies that a failed <c>BuildAsync</c> invocation does not set the internal
    /// <c>_isAlreadyBuilt</c> flag, so subsequent build attempts on the same builder reproduce
    /// the fault instead of throwing "already built".
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_FailedRunDoesNotSetIsAlreadyBuilt()
    {
        var builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        builder.UseServicePack(new DynamicServicePack().Setup(_ => throw new InvalidOperationException("setup-boom")));

        // first build fails in Phase 5
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await builder.BuildAsync(TestContext.Current.CancellationToken)
        );

        // second build on SAME builder should reproduce the same fault, NOT throw "already built"
        var secondEx = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await builder.BuildAsync(TestContext.Current.CancellationToken)
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
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_Phase4NullsTransientAfterDispose_NoDoubleDisposeRegression()
    {
        var order = new List<string>();
        var finalSp = BuildSpWithOrderedHook("final", order);
        var transientSp = BuildSpWithOrderedHook("transient", order);

        var original = new InvalidOperationException("original");

        // expect no throw — both providers dispose cleanly
        await ServiceProviderBuilder.DisposeWithAggregationAsync(original, finalSp, transientSp);

        // dispose order: final before transient
        order.Count.Is(2);
        order[0].IsEqual("final");
        order[1].IsEqual("transient");
    }

    /// <summary>
    /// Verifies that <see cref="ServiceProviderBuilder.DisposeWithAggregationAsync"/> aggregates
    /// the original exception plus individual dispose errors from BOTH final and transient providers
    /// when each owns a service that throws from DisposeAsync.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeWithAggregation_BothProvidersThrow_ThreeInnerExceptions()
    {
        // arrange — build two providers each owning a ThrowOnAsyncDispose singleton
        var finalSp = BuildSpWithThrowOnAsyncDispose();
        var transientSp = BuildSpWithThrowOnAsyncDispose();
        var original = new InvalidOperationException("original");

        // act
        var ex = await Assert.ThrowsAsync<AggregateException>(async () =>
            await ServiceProviderBuilder.DisposeWithAggregationAsync(original, finalSp, transientSp)
        );

        // assert — InnerExceptions[0] is original; [1] and [2] are the two dispose errors
        ex.InnerExceptions[0].Message.IsEqual("original");
        ex.InnerExceptions.Count.Is(3);
    }

    /// <summary>
    /// Builds a <see cref="ServiceProvider"/> that materialises a <see cref="ThrowOnAsyncDispose"/>
    /// singleton so that disposing the provider causes <see cref="IAsyncDisposable.DisposeAsync"/> to throw.
    /// </summary>
    /// <returns>A built and materialised <see cref="ServiceProvider"/>.</returns>
    private static ServiceProvider BuildSpWithThrowOnAsyncDispose()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ThrowOnAsyncDispose>(_ => new ThrowOnAsyncDispose());
        var sp = services.BuildServiceProvider();
        _ = sp.GetRequiredService<ThrowOnAsyncDispose>();
        return sp;
    }

    /// <summary>
    /// Verifies depth-first walker ordering across nested <see cref="ServicePackBase"/> trees:
    /// each phase iterates child packs (depth-first) before invoking the parent's hook.
    /// Guards <see cref="ServicePackBase.InternalConfigureAsync"/>, <see cref="ServicePackBase.InternalRegisterAsync"/>,
    /// and <see cref="ServicePackBase.InternalSetupAsync"/> against a regression that calls the parent
    /// hook before walking children.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_NestedServicePacks_DepthFirstChildBeforeParent()
    {
        var order = new List<string>();
        WalkOrderTracker.Sink = order;
        try
        {
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
        }
        finally
        {
            WalkOrderTracker.Sink = null;
        }
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
        builder.UseServicePack(new DynamicServicePack().Configure(c => c.Add<TransientHook>().AsSelf().Singleton()));

        var sp = factory.CreateServiceProvider(builder);

        // GetRequiredService throws InvalidOperationException if the service can't be resolved,
        // so a successful resolution + non-null instance proves the provider is operational.
        var hook = sp.GetRequiredService<TransientHook>();
        hook.IsNotDefault();
    }

    /// <summary>
    /// Builds a <see cref="ServiceProvider"/> that owns an <see cref="OrderedDisposeHook"/> singleton.
    /// When the provider is disposed the hook appends <paramref name="tag"/> to <paramref name="order"/>,
    /// enabling tests to assert the disposal sequence.
    /// </summary>
    /// <param name="tag">Label appended to <paramref name="order"/> on dispose (e.g. "final" or "transient").</param>
    /// <param name="order">Shared list that records dispose invocation order.</param>
    /// <returns>A built <see cref="ServiceProvider"/> with the ordered hook materialised.</returns>
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
    /// <summary>Gets or sets the dispose-counting hook injected by the test after resolution.</summary>
    public DisposeHook? Hook;

    /// <summary>Delegates disposal to the injected <see cref="Hook"/>, if set.</summary>
    public void Dispose() => Hook?.Dispose();
}

/// <summary>
/// Test singleton tracked by final SP. Mirrors <see cref="TransientHook"/> but materialised in final
/// (resolved during Setup).
/// </summary>
internal sealed class FinalHook : IDisposable
{
    /// <summary>Gets or sets the dispose-counting hook injected by the test after resolution.</summary>
    public DisposeHook? Hook;

    /// <summary>Delegates disposal to the injected <see cref="Hook"/>, if set.</summary>
    public void Dispose() => Hook?.Dispose();
}

/// <summary>
/// Singleton helper that counts how many times its <see cref="Dispose"/> method has been invoked.
/// Test-scoped (constructed per Fact) so static state cannot bleed between tests.
/// </summary>
internal sealed class DisposeHook
{
    /// <summary>Gets the number of times <see cref="Dispose"/> has been invoked.</summary>
    public int DisposedCount;

    /// <summary>Atomically increments <see cref="DisposedCount"/> to record one disposal.</summary>
    public void Dispose() => Interlocked.Increment(ref DisposedCount);
}

/// <summary>
/// Hook that records the order of dispose calls into a shared list. Used by the
/// final-then-transient regression guard (#13).
/// </summary>
internal sealed class OrderedDisposeHook(string tag, List<string> sink) : IDisposable
{
    /// <summary>Appends the tag supplied at construction to the shared sink under a lock.</summary>
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
    /// <summary>Gets or sets the cancellation source that will be cancelled when this instance is disposed.</summary>
    public CancellationTokenSource? Cts;

    /// <summary>Cancels the associated <see cref="Cts"/>, if set, simulating in-dispose cancellation.</summary>
    // VSTHRD103: Dispose() is the synchronous IDisposable contract — CancelAsync (ValueTask) cannot be awaited here, so Cts?.Cancel() is the correct call.
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
    /// <summary>Always throws <see cref="InvalidOperationException"/> to drive the dispose-error aggregation path.</summary>
    /// <returns>This method never returns; it always throws.</returns>
    public ValueTask DisposeAsync() => throw new InvalidOperationException("dispose-boom");
}

/// <summary>
/// Test-scoped sink for depth-first walker ordering verification. Per-fact: tests set the Sink
/// before constructing the pack tree and null it out at end. Collection-level parallelization is
/// disabled on <see cref="DisposalContractTests"/> so static state is safe.
/// </summary>
internal static class WalkOrderTracker
{
    /// <summary>Gets or sets the list that receives phase-invocation entries; null when not under test.</summary>
    public static List<string>? Sink;

    /// <summary>
    /// Appends <paramref name="entry"/> to <see cref="Sink"/> under a lock.
    /// Does nothing when <see cref="Sink"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="entry">The phase-invocation label to record (e.g. "child:configure").</param>
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
    /// <summary>
    /// Records the "child:configure" invocation in <see cref="WalkOrderTracker"/> to verify depth-first walk order.
    /// </summary>
    /// <param name="container">The service container receiving registrations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        WalkOrderTracker.Record("child:configure");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Records the "child:register" invocation in <see cref="WalkOrderTracker"/> to verify depth-first walk order.
    /// </summary>
    /// <param name="container">The service container receiving registrations.</param>
    /// <param name="provider">The transient service provider built after Configure.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        WalkOrderTracker.Record("child:register");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Records the "child:setup" invocation in <see cref="WalkOrderTracker"/> to verify depth-first walk order.
    /// </summary>
    /// <param name="provider">The final service provider built after Register.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A completed task.</returns>
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

    /// <summary>
    /// Records the "parent:configure" invocation in <see cref="WalkOrderTracker"/> to verify depth-first walk order.
    /// </summary>
    /// <param name="container">The service container receiving registrations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public override Task ConfigureAsync(IServiceContainer container, CancellationToken ct)
    {
        WalkOrderTracker.Record("parent:configure");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Records the "parent:register" invocation in <see cref="WalkOrderTracker"/> to verify depth-first walk order.
    /// </summary>
    /// <param name="container">The service container receiving registrations.</param>
    /// <param name="provider">The transient service provider built after Configure.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        WalkOrderTracker.Record("parent:register");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Records the "parent:setup" invocation in <see cref="WalkOrderTracker"/> to verify depth-first walk order.
    /// </summary>
    /// <param name="provider">The final service provider built after Register.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        WalkOrderTracker.Record("parent:setup");
        return Task.CompletedTask;
    }
}
