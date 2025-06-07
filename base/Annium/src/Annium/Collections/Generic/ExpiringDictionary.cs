using System.Collections.Concurrent;
using System.Collections.Generic;
using NodaTime;

namespace Annium.Collections.Generic;

/// <summary>
/// Represents a dictionary of key-value pairs that expire after a specified duration.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
public class ExpiringDictionary<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// The time provider used to determine the current time.
    /// </summary>
    private readonly ITimeProvider _timeProvider;

    /// <summary>
    /// The underlying dictionary storing the key-value pairs and their expiration times.
    /// </summary>
    private readonly ConcurrentDictionary<TKey, Item> _data = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiringDictionary{TKey, TValue}"/> class with the specified time provider.
    /// </summary>
    /// <param name="timeProvider">The time provider to use for determining expiration times.</param>
    public ExpiringDictionary(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Adds a key-value pair to the dictionary with the specified time-to-live duration.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <param name="ttl">The duration after which the element will expire.</param>
    public void Add(TKey key, TValue value, Duration ttl)
    {
        var item = new Item(value, _timeProvider.Now + ttl);
        _data.AddOrUpdate(key, item, (_, _) => item);
    }

    /// <summary>
    /// Attempts to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
    /// <returns>True if the key was found; otherwise, false.</returns>
    public bool TryGet(TKey key, out TValue value)
    {
        Cleanup();

        var result = _data.TryGetValue(key, out var item);
        value = item is null ? default! : item.Value;

        return result;
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the key is not found in the dictionary.</exception>
    public TValue Get(TKey key)
    {
        Cleanup();

        if (!_data.TryGetValue(key, out var item))
            throw new KeyNotFoundException($"Key {key} is missing in collection");

        return item.Value;
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
    public bool ContainsKey(TKey key)
    {
        Cleanup();

        return _data.ContainsKey(key);
    }

    /// <summary>
    /// Removes the value with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
    /// <returns>True if the element is successfully removed; otherwise, false.</returns>
    public bool Remove(TKey key, out TValue value)
    {
        Cleanup();

        var result = _data.TryRemove(key, out var item);
        value = item is null ? default! : item.Value;

        return result;
    }

    /// <summary>
    /// Removes all keys and values from the dictionary.
    /// </summary>
    public void Clear()
    {
        _data.Clear();
    }

    /// <summary>
    /// Removes expired key-value pairs from the dictionary.
    /// </summary>
    private void Cleanup()
    {
        var now = _timeProvider.Now;
        var pairs = _data.ToArray();
        foreach (var (key, value) in pairs)
        {
            if (value.Expires < now)
                _data.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Represents an item in the dictionary with its value and expiration time.
    /// </summary>
    private record Item(TValue Value, Instant Expires);
}
