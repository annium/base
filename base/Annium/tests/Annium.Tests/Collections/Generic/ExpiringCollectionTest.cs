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
        var collection = new ExpiringCollection<int>(timeProvider);
        var ttl = Duration.FromSeconds(5);

        // act
        Parallel.ForEach(Enumerable.Range(0, 100), (x, _, _) => collection.Add(x, ttl));

        // assert
        foreach (var value in Enumerable.Range(0, 100))
            collection.Contains(value).IsTrue();
    }

    /// <summary>
    /// Verifies that the Contains method works correctly, including after expiration.
    /// </summary>
    [Fact]
    public void Contains_Works()
    {
        // arrange
        var (timeManager, timeProvider) = GetTimeTools();
        var collection = new ExpiringCollection<Guid>(timeProvider);
        var value = Guid.NewGuid();
        var ttl = Duration.FromSeconds(5);
        collection.Add(value, ttl);

        // assert
        collection.Contains(value).IsTrue();
        timeManager.SetNow(timeProvider.Now + ttl);
        collection.Contains(value).IsTrue();
        timeManager.SetNow(timeProvider.Now + ttl + Duration.FromMilliseconds(1));
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
        var collection = new ExpiringCollection<Guid>(timeProvider);
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
}
