using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Collections.Generic;
using Annium.Core.Runtime.Time;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Tests.Collections.Generic;

/// <summary>
/// Contains unit tests for <see cref="ExpiringDictionary{TKey, TValue}"/> to verify expiration and dictionary behavior.
/// </summary>
public class ExpiringDictionaryTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiringDictionaryTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ExpiringDictionaryTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that adding elements to the dictionary works correctly.
    /// </summary>
    [Fact]
    public void Add_Works()
    {
        // arrange
        var (_, timeProvider) = GetTimeTools();
        var collection = new ExpiringDictionary<int, string>(timeProvider);
        var ttl = Duration.FromSeconds(5);

        // act
        Parallel.ForEach(Enumerable.Range(0, 100), (x, _, _) => collection.Add(x, $"val:{x}", ttl));

        // assert
        foreach (var value in Enumerable.Range(0, 100))
            collection.ContainsKey(value).IsTrue();
    }

    /// <summary>
    /// Verifies that getting elements from the dictionary works correctly, including after expiration.
    /// </summary>
    [Fact]
    public void Get_Works()
    {
        // arrange
        var (timeManager, timeProvider) = GetTimeTools();
        var collection = new ExpiringDictionary<Guid, string>(timeProvider);
        var key = Guid.NewGuid();
        var value = "secret";
        var ttl = Duration.FromSeconds(5);
        collection.Add(key, value, ttl);

        // assert
        collection.Get(key).Is(value);
        timeManager.SetNow(timeProvider.Now + ttl);
        collection.Get(key).Is(value);
        timeManager.SetNow(timeProvider.Now + ttl + Duration.FromMilliseconds(1));
        Wrap.It(() => collection.Get(key)).Throws<KeyNotFoundException>();
    }

    /// <summary>
    /// Verifies that TryGet works correctly, including after expiration.
    /// </summary>
    [Fact]
    public void TryGet_Works()
    {
        // arrange
        var (timeManager, timeProvider) = GetTimeTools();
        var collection = new ExpiringDictionary<Guid, string>(timeProvider);
        var key = Guid.NewGuid();
        var value = "secret";
        var ttl = Duration.FromSeconds(5);
        collection.Add(key, value, ttl);

        // assert
        collection.TryGet(key, out var val).IsTrue();
        val.Is(value);
        timeManager.SetNow(timeProvider.Now + ttl);
        collection.Get(key).Is(value);
        timeManager.SetNow(timeProvider.Now + ttl + Duration.FromMilliseconds(1));
        Wrap.It(() => collection.Get(key)).Throws<KeyNotFoundException>();
    }

    /// <summary>
    /// Verifies that ContainsKey works correctly, including after expiration.
    /// </summary>
    [Fact]
    public void ContainsKey_Works()
    {
        // arrange
        var (timeManager, timeProvider) = GetTimeTools();
        var collection = new ExpiringDictionary<Guid, string>(timeProvider);
        var key = Guid.NewGuid();
        var ttl = Duration.FromSeconds(5);
        collection.Add(key, "secret", ttl);

        // assert
        collection.ContainsKey(key).IsTrue();
        timeManager.SetNow(timeProvider.Now + ttl);
        collection.ContainsKey(key).IsTrue();
        timeManager.SetNow(timeProvider.Now + ttl + Duration.FromMilliseconds(1));
        collection.ContainsKey(key).IsFalse();
    }

    /// <summary>
    /// Verifies that removing elements from the dictionary works correctly, including after expiration.
    /// </summary>
    [Fact]
    public void Remove_Works()
    {
        // arrange
        var (timeManager, timeProvider) = GetTimeTools();
        var collection = new ExpiringDictionary<Guid, string>(timeProvider);
        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();
        var ttl = Duration.FromSeconds(5);
        collection.Add(key1, "a", ttl);
        collection.Add(key2, "b", ttl * 2);

        // assert
        collection.Remove(key2, out _).IsTrue();
        collection.ContainsKey(key1).IsTrue();
        collection.ContainsKey(key2).IsFalse();
        timeManager.SetNow(timeProvider.Now + ttl + Duration.FromMilliseconds(1));
        collection.Remove(key2, out _).IsFalse();
        collection.ContainsKey(key1).IsFalse();
        collection.ContainsKey(key2).IsFalse();
    }

    /// <summary>
    /// Gets the time manager and time provider for testing.
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
