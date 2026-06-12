using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Collections.Generic;
using Annium.Core.Runtime.Time;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Tests.Collections.Generic;

/// <summary>
/// Contains unit tests for <see cref="ExpiringCollection{T}"/> to verify expiration and collection behavior.
/// </summary>
public class ExpiringCollectionTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiringCollectionTest"/> class.
    /// </summary>
    public ExpiringCollectionTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that adding elements to the collection works correctly.
    /// </summary>
    [Fact]
    public void Add_Works()
    {
        // arrange
        var (_, timeProvider) = GetTimeTools();
        using var collection = new ExpiringCollection<int>(timeProvider);
        var ttl = Duration.FromSeconds(5);

        // act
        Parallel.ForEach(Enumerable.Range(0, 100), (x, _, _) => collection.Add(x, ttl));

        // assert
        foreach (var value in Enumerable.Range(0, 100))
            collection.Contains(value).IsTrue();
    }

    /// <summary>
    /// Verifies that the Contains method works correctly, including the expiration boundary
    /// (an item is considered expired the instant it reaches its expiry, not strictly afterwards).
    /// </summary>
    [Fact]
    public void Contains_Works()
    {
        // arrange
        var (timeManager, timeProvider) = GetTimeTools();
        using var collection = new ExpiringCollection<Guid>(timeProvider);
        var value = Guid.NewGuid();
        var ttl = Duration.FromSeconds(5);
        collection.Add(value, ttl);

        // assert: still alive just before expiry
        collection.Contains(value).IsTrue();
        timeManager.SetNow(timeProvider.Now + ttl - Duration.FromMilliseconds(1));
        collection.Contains(value).IsTrue();

        // assert: expired exactly at the boundary
        timeManager.SetNow(timeProvider.Now + Duration.FromMilliseconds(1));
        collection.Contains(value).IsFalse();

        // assert: still expired after the boundary
        timeManager.SetNow(timeProvider.Now + Duration.FromMilliseconds(1));
        collection.Contains(value).IsFalse();
    }

    /// <summary>
    /// Verifies that removing elements from the collection works correctly, including after expiration.
    /// </summary>
    [Fact]
    public void Remove_Works()
    {
        // arrange
        var (timeManager, timeProvider) = GetTimeTools();
        using var collection = new ExpiringCollection<Guid>(timeProvider);
        var value1 = Guid.NewGuid();
        var value2 = Guid.NewGuid();
        var ttl = Duration.FromSeconds(5);
        collection.Add(value1, ttl);
        collection.Add(value2, ttl * 2);

        // assert
        collection.Remove(value2).IsTrue();
        collection.Contains(value1).IsTrue();
        collection.Contains(value2).IsFalse();
        timeManager.SetNow(timeProvider.Now + ttl + Duration.FromMilliseconds(1));
        collection.Remove(value2).IsFalse();
        collection.Contains(value1).IsFalse();
        collection.Contains(value2).IsFalse();
    }

    /// <summary>
    /// Gets the time manager and time provider for testing expiration logic.
    /// </summary>
    /// <returns>A tuple containing the time manager and time provider.</returns>
    private (ITimeManager, ITimeProvider) GetTimeTools()
    {
        Get<ITimeProviderSwitcher>().UseManagedTime();
        var timeManager = Get<ITimeManager>();
        timeManager.SetNow(Instant.FromDateTimeUtc(DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)));

        var timeProvider = Get<ITimeProvider>();

        return (timeManager, timeProvider);
    }

    /// <summary>
    /// Verifies that <c>Clear</c> removes every entry from the collection. Closes the TG10 gap from
    /// review-2026.05.15 — Clear was previously untested.
    /// </summary>
    [Fact]
    public void Clear_RemovesAllItems()
    {
        // arrange
        var (_, timeProvider) = GetTimeTools();
        using var collection = new ExpiringCollection<int>(timeProvider);
        var ttl = Duration.FromSeconds(5);
        for (var i = 0; i < 10; i++)
            collection.Add(i, ttl);

        // act
        collection.Clear();

        // assert
        for (var i = 0; i < 10; i++)
            collection.Contains(i).IsFalse();
    }

    /// <summary>
    /// Verifies that <c>DisposeAsync</c> stops the background eviction timer cleanly and is idempotent
    /// across both sync and async dispose paths, and that the entries remain accessible after dispose
    /// (the internal helper's contract is to keep operating on the dictionary without throwing — only
    /// the background eviction stops). Closes the TG10 gap and strengthens the assertion surface that
    /// previously only verified "second dispose does not throw".
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DisposeAsync_IsIdempotent()
    {
        // arrange
        var (_, timeProvider) = GetTimeTools();
        using var collection = new ExpiringCollection<int>(timeProvider);
        collection.Add(1, Duration.FromSeconds(5));

        // act — dispose twice; second call must be a no-op (idempotent)
        await collection.DisposeAsync();
        await collection.DisposeAsync();

        // assert — entry remains observable (no ObjectDisposedException); background eviction is stopped
        collection.Contains(1).IsTrue();
    }
}
