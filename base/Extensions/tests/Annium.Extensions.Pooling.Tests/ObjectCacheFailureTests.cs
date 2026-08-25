using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
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
