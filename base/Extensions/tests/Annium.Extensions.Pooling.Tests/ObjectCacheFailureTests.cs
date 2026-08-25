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
    /// Callers already waiting on the in-flight creation observe the failure instead of hanging on a
    /// placeholder that will never be filled.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAsync_ConcurrentWhileFactoryThrows_AllFail()
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

        // assert - bounded, because the failure being pinned is an unbounded wait
        var all = Task.WhenAll(attempts);
        var completed = await Task.WhenAny(
            all,
            Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken)
        );
        (completed == all).IsTrue("waiters must not hang on a creation that failed");
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
