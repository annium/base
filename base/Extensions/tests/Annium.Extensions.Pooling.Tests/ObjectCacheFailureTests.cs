using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Testing;
using OneOf;
using Xunit;

namespace Annium.Extensions.Pooling.Tests;

/// <summary>
/// Tests for what a failing factory leaves behind. The cache inserts a placeholder before calling the
/// factory so concurrent callers for the same key wait rather than all building one, which means a factory
/// that throws must both release those waiters and remove the placeholder — otherwise the key is either
/// wedged forever or permanently poisoned.
/// </summary>
public class ObjectCacheFailureTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectCacheFailureTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public ObjectCacheFailureTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.Add<FlakyProvider>().AsSelf().Singleton();
            container.AddObjectCache<string, Flaky, FlakyProvider>(
                Annium.Core.DependencyInjection.ServiceLifetime.Singleton
            );
            container.Add<BrittleProvider>().AsSelf().Singleton();
            container.Add<ReferencingProvider>().AsSelf().Singleton();
            container.Add<SlowProvider>().AsSelf().Singleton();
            container.Add<SuspendableProvider>().AsSelf().Singleton();
            container.AddObjectCache<string, Suspendable, SuspendableProvider>(
                Annium.Core.DependencyInjection.ServiceLifetime.Singleton
            );
            container.AddObjectCache<string, Slow, SlowProvider>(
                Annium.Core.DependencyInjection.ServiceLifetime.Singleton
            );
            container.AddObjectCache<string, Referenced, ReferencingProvider>(
                Annium.Core.DependencyInjection.ServiceLifetime.Singleton
            );
            container.AddObjectCache<string, Brittle, BrittleProvider>(
                Annium.Core.DependencyInjection.ServiceLifetime.Singleton
            );
        });
    }

    /// <summary>
    /// A failing factory surfaces its own failure to the caller.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAsync_FactoryThrows_Propagates()
    {
        // arrange
        var cache = Get<IObjectCache<string, Flaky>>();

        // act & assert
        await Wrap.It(async () => await cache.GetAsync("boom")).ThrowsAsync<InvalidOperationException>();
    }

    /// <summary>
    /// After a failure the key is not poisoned: the next request calls the factory again and can succeed.
    /// A placeholder left in place would make the key permanently unusable.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAsync_AfterFactoryThrew_RetriesFreshly()
    {
        // arrange
        var provider = Get<FlakyProvider>();
        var cache = Get<IObjectCache<string, Flaky>>();
        provider.FailNext();
        await Wrap.It(async () => await cache.GetAsync("key")).ThrowsAsync<InvalidOperationException>();

        // act - the factory now behaves
        await using var reference = await cache.GetAsync("key", TestContext.Current.CancellationToken);

        // assert
        reference.Value.Key.Is("key");
        provider.Calls.Is(2, "the second request must reach the factory rather than a cached failure");
    }

    /// <summary>
    /// Nobody hangs when the one in-flight creation fails, and the failure is actually reported to at
    /// least the callers that were waiting on it. Callers arriving after the failed entry was dropped
    /// legitimately create a fresh one and may succeed — that is the retry, not a swallowed error.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAsync_ConcurrentWhileFactoryThrows_NobodyHangsAndTheFailureIsReported()
    {
        // arrange
        var provider = Get<FlakyProvider>();
        var cache = Get<IObjectCache<string, Flaky>>();
        provider.FailNext();

        // act - several callers race for one key while its only creation attempt fails
        var attempts = Enumerable
            .Range(0, 5)
            .Select(_ => Task.Run(async () => await cache.GetAsync("shared"), TestContext.Current.CancellationToken))
            .ToArray();

        // assert - bounded first, because the failure being pinned is an unbounded wait
        var all = Task.WhenAll(attempts);
#pragma warning disable VSTHRD003
        var completed = await Task.WhenAny(
            all,
            Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken)
        );
        (completed == all).IsTrue("waiters must not hang on a creation that failed");

        // and the failure reached a caller rather than being swallowed: finishing quickly would be no
        // better than hanging if everyone silently came back with a value the factory never produced
        var failures = 0;
        foreach (var attempt in attempts)
        {
            try
            {
                await using var reference = await attempt;
                reference.Value.Key.Is("shared", "a caller that succeeded must hold a real value");
            }
            catch (InvalidOperationException)
            {
                failures++;
            }
        }
#pragma warning restore VSTHRD003

        (failures > 0).IsTrue("the factory failure must be reported to the callers waiting on it");
    }

    /// <summary>
    /// A provider that hands back its own reference does not thereby opt out of the cache's reference
    /// counting. The count was incremented for the creating caller either way, but that caller was given
    /// the provider's handle, which does not release it - so the entry never dropped to zero references
    /// and was never suspended.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAsync_ProviderReturnsItsOwnReference_StillSuspendsWhenReleased()
    {
        // arrange
        var provider = Get<ReferencingProvider>();
        var cache = Get<IObjectCache<string, Referenced>>();

        // act - taken and released by the caller that created it
        await using (await cache.GetAsync("key", TestContext.Current.CancellationToken)) { }

        // assert
        await Expect.ToAsync(() => provider.Suspends.Is(1, "releasing the last reference must suspend the entry"));

        // and the provider's own reference belongs to the entry, so it goes when the cache does
        await ((IAsyncDisposable)cache).DisposeAsync();
        provider.ReferenceDisposals.Is(1, "the cache must release the reference its provider handed back");
    }

    /// <summary>
    /// A caller waiting on somebody else's slow creation can give up. GetAsync takes a token and used to
    /// consult it only on the path that creates the value - everyone who arrived second waited for that
    /// creation however long it took, whatever their own deadline said.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAsync_WaitingOnAnotherCaller_CanBeCancelled()
    {
        // arrange - the first caller holds the key while its creation crawls
        var provider = Get<SlowProvider>();
        var cache = Get<IObjectCache<string, Slow>>();
        var first = Task.Run(
            async () => await cache.GetAsync("shared", TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken
        );
        await provider.Started;

        // act - the second arrives with a deadline of its own
        using var cts = new CancellationTokenSource();
        var second = cache.GetAsync("shared", cts.Token);
        await cts.CancelAsync();

#pragma warning disable VSTHRD003
        try
        {
            // assert - bounded, because the failure being pinned is an unbounded wait
            var completed = await Task.WhenAny(
                second,
                Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)
            );
            (completed == second).IsTrue("a waiter that gave up must not be held by another caller");
            await Wrap.It(async () => await second).ThrowsAsync<OperationCanceledException>();
        }
        finally
        {
            // whatever happened above, the creation has to be let go: the cache's own disposal waits for
            // it, so leaving it held would hang the run rather than fail this test
            provider.Finish();
            await using var _ = await first;
        }
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// A provider that fails to suspend a value does not wedge its key. The gate is taken to release a
    /// reference and was only handed back after the provider had been asked to suspend, so a provider that
    /// threw kept it - and every later use of that key, including the cache's own disposal, waited on a
    /// gate nobody would ever hand back.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ReleaseAsync_SuspendThrows_DoesNotWedgeTheKey()
    {
        // arrange
        var cache = Get<IObjectCache<string, Suspendable>>();
        await (await cache.GetAsync("key", TestContext.Current.CancellationToken)).DisposeAsync();

        // act & assert - the key is still usable, bounded because the failure pinned is an unbounded wait
        var again = cache.GetAsync("key", TestContext.Current.CancellationToken);
#pragma warning disable VSTHRD003
        var completed = await Task.WhenAny(
            again,
            Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)
        );
        (completed == again).IsTrue("a failed suspend must not wedge the key it was for");
        await using var _ = await again;
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// Disposing the cache reaches every entry, even when disposing one of them fails. Each entry holds a
    /// resource of its own, so stopping at the first failure leaks all the ones after it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DisposeAsync_OneEntryFailsToDispose_TheRestAreStillDisposed()
    {
        // arrange - every entry records the attempt and then throws, so the count is what is being pinned
        // and the outcome does not depend on which entry the cache happens to reach first
        var provider = Get<BrittleProvider>();
        var cache = Get<IObjectCache<string, Brittle>>();
        foreach (var key in new[] { "a", "b", "c" })
            await (await cache.GetAsync(key, TestContext.Current.CancellationToken)).DisposeAsync();

        // act
        await ((IAsyncDisposable)cache).DisposeAsync();

        // assert
        provider.DisposeAttempts.Is(3, "every entry must be disposed, not just the ones before the first failure");
    }

    /// <summary>
    /// A reference released after the whole cache is gone is a no-op, not an error.
    /// Nothing is left to release it into, and the caller did nothing wrong by holding it -
    /// this is the ordinary order in which a host shuts down.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ReleaseReference_AfterCacheDisposed_IsSilent()
    {
        // arrange
        var cache = Get<IObjectCache<string, Flaky>>();
        var reference = await cache.GetAsync("ok", TestContext.Current.CancellationToken);

        // act - the cache goes first, the reference the caller still holds goes after
        await ((IAsyncDisposable)cache).DisposeAsync();
        await reference.DisposeAsync();

        // assert
        Logs.Where(x => x.Level >= LogLevel.Error)
            .IsEmpty("releasing into a disposed cache must be a no-op, not something the host has to explain");
    }
}

/// <summary>
/// A cached value carrying the key it was built for.
/// </summary>
/// <param name="Key">The key this value was created for.</param>
public sealed record Flaky(string Key);

/// <summary>
/// Provider whose factory fails on demand, and counts how often it was called.
/// </summary>
public class FlakyProvider : ObjectCacheProvider<string, Flaky>
{
    /// <summary>
    /// Gets how many times the factory has been invoked.
    /// </summary>
    public int Calls => Volatile.Read(ref _calls);

    /// <summary>
    /// Number of factory invocations so far.
    /// </summary>
    private int _calls;

    /// <summary>
    /// Whether the next factory call should fail.
    /// </summary>
    private int _failNext;

    /// <summary>
    /// Makes the next factory call throw.
    /// </summary>
    public void FailNext() => Volatile.Write(ref _failNext, 1);

    /// <summary>
    /// Creates a value, or throws when armed to fail.
    /// </summary>
    /// <param name="id">The key to create a value for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created value.</returns>
    public override async Task<OneOf<Flaky, IDisposableReference<Flaky>>> CreateAsync(string id, CancellationToken ct)
    {
        Interlocked.Increment(ref _calls);
        await Task.Delay(10, ct);

        if (id == "boom" || Interlocked.Exchange(ref _failNext, 0) == 1)
            throw new InvalidOperationException($"cannot create '{id}'");

        return new Flaky(id);
    }
}

/// <summary>
/// A cached value whose disposal always fails.
/// </summary>
/// <param name="Key">The key this value was created for.</param>
public sealed record Brittle(string Key);

/// <summary>
/// Provider that counts disposal attempts and fails every one of them.
/// </summary>
public class BrittleProvider : ObjectCacheProvider<string, Brittle>
{
    /// <summary>
    /// Gets how many values the cache has tried to dispose.
    /// </summary>
    public int DisposeAttempts => Volatile.Read(ref _disposeAttempts);

    /// <summary>
    /// Number of disposal attempts so far.
    /// </summary>
    private int _disposeAttempts;

    /// <summary>
    /// Creates a value.
    /// </summary>
    /// <param name="id">The key to create a value for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created value.</returns>
    public override Task<OneOf<Brittle, IDisposableReference<Brittle>>> CreateAsync(string id, CancellationToken ct) =>
        Task.FromResult(OneOf<Brittle, IDisposableReference<Brittle>>.FromT0(new Brittle(id)));

    /// <summary>
    /// Records the attempt, then fails.
    /// </summary>
    /// <param name="key">The key identifying the value.</param>
    /// <param name="value">The value being disposed.</param>
    /// <returns>Nothing - this always throws.</returns>
    public override Task DisposeAsync(string key, Brittle value)
    {
        Interlocked.Increment(ref _disposeAttempts);

        throw new InvalidOperationException($"cannot dispose '{key}'");
    }
}

/// <summary>
/// A cached value the provider wraps in a reference of its own.
/// </summary>
/// <param name="Key">The key this value was created for.</param>
public sealed record Referenced(string Key);

/// <summary>
/// Provider that returns its own disposable reference rather than a bare value.
/// </summary>
public class ReferencingProvider : ObjectCacheProvider<string, Referenced>
{
    /// <summary>
    /// Gets how many times an entry was suspended.
    /// </summary>
    public int Suspends => Volatile.Read(ref _suspends);

    /// <summary>
    /// Gets how many times the reference this provider handed back was released.
    /// </summary>
    public int ReferenceDisposals => Volatile.Read(ref _referenceDisposals);

    /// <summary>
    /// Number of suspends observed.
    /// </summary>
    private int _suspends;

    /// <summary>
    /// Number of times the handed-back reference was released.
    /// </summary>
    private int _referenceDisposals;

    /// <summary>
    /// Creates a value wrapped in the provider's own reference.
    /// </summary>
    /// <param name="id">The key to create a value for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The provider's reference to the created value.</returns>
    public override Task<OneOf<Referenced, IDisposableReference<Referenced>>> CreateAsync(
        string id,
        CancellationToken ct
    ) =>
        Task.FromResult(
            OneOf<Referenced, IDisposableReference<Referenced>>.FromT1(
                Disposable.Reference(
                    new Referenced(id),
                    () =>
                    {
                        Interlocked.Increment(ref _referenceDisposals);

                        return ValueTask.CompletedTask;
                    }
                )
            )
        );

    /// <summary>
    /// Records the suspend.
    /// </summary>
    /// <param name="key">The key identifying the value.</param>
    /// <param name="value">The suspended value.</param>
    /// <returns>A task representing the asynchronous suspend operation.</returns>
    public override Task SuspendAsync(string key, Referenced value)
    {
        Interlocked.Increment(ref _suspends);

        return Task.CompletedTask;
    }
}

/// <summary>
/// A cached value whose creation is held open by the test.
/// </summary>
/// <param name="Key">The key this value was created for.</param>
public sealed record Slow(string Key);

/// <summary>
/// Provider whose creation blocks until the test lets it finish.
/// </summary>
public class SlowProvider : ObjectCacheProvider<string, Slow>
{
    /// <summary>
    /// Gets a task that completes once creation has begun.
    /// </summary>
    public Task Started => _started.Task;

    /// <summary>
    /// Signals that creation has begun.
    /// </summary>
    private readonly TaskCompletionSource _started = new();

    /// <summary>
    /// Held until the test releases it.
    /// </summary>
    private readonly TaskCompletionSource _finish = new();

    /// <summary>
    /// Lets the creation complete.
    /// </summary>
    public void Finish() => _finish.TrySetResult();

    /// <summary>
    /// Creates a value, once allowed to.
    /// </summary>
    /// <param name="id">The key to create a value for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created value.</returns>
    public override async Task<OneOf<Slow, IDisposableReference<Slow>>> CreateAsync(string id, CancellationToken ct)
    {
        _started.TrySetResult();
#pragma warning disable VSTHRD003
        await _finish.Task;
#pragma warning restore VSTHRD003

        return new Slow(id);
    }
}

/// <summary>
/// A cached value whose suspension fails.
/// </summary>
/// <param name="Key">The key this value was created for.</param>
public sealed record Suspendable(string Key);

/// <summary>
/// Provider that cannot suspend what it created.
/// </summary>
public class SuspendableProvider : ObjectCacheProvider<string, Suspendable>
{
    /// <summary>
    /// Creates a value.
    /// </summary>
    /// <param name="id">The key to create a value for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created value.</returns>
    public override Task<OneOf<Suspendable, IDisposableReference<Suspendable>>> CreateAsync(
        string id,
        CancellationToken ct
    ) => Task.FromResult<OneOf<Suspendable, IDisposableReference<Suspendable>>>(new Suspendable(id));

    /// <summary>
    /// Fails to suspend.
    /// </summary>
    /// <param name="key">The key identifying the value.</param>
    /// <param name="value">The value to suspend.</param>
    /// <returns>Nothing - this always throws.</returns>
    public override Task SuspendAsync(string key, Suspendable value) =>
        throw new InvalidOperationException($"cannot suspend '{key}'");
}
