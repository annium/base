using System.Collections.Concurrent;
using NodaTime;

namespace Annium.Collections.Generic;

/// <summary>
/// Represents a collection of items that expire after a specified duration.
/// </summary>
/// <typeparam name="T">The type of the items in the collection.</typeparam>
public class ExpiringCollection<T>
    where T : notnull
{
    /// <summary>
    /// The time provider used to determine the current time.
    /// </summary>
    private readonly ITimeProvider _timeProvider;

    /// <summary>
    /// The underlying dictionary storing the items and their expiration times.
    /// </summary>
    private readonly ConcurrentDictionary<T, Instant> _data = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiringCollection{T}"/> class with the specified time provider.
    /// </summary>
    /// <param name="timeProvider">The time provider to use for determining expiration times.</param>
    public ExpiringCollection(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    /// <summary>
    /// Adds an item to the collection with the specified time-to-live duration.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="ttl">The duration after which the item will expire.</param>
    public void Add(T item, Duration ttl)
    {
        var expires = _timeProvider.Now + ttl;
        _data.AddOrUpdate(item, expires, (_, _) => expires);
    }

    /// <summary>
    /// Checks if the collection contains the specified item.
    /// </summary>
    /// <param name="item">The item to check for.</param>
    /// <returns>True if the item is present and not expired; otherwise, false.</returns>
    public bool Contains(T item)
    {
        Cleanup();

        return _data.ContainsKey(item);
    }

    /// <summary>
    /// Removes the specified item from the collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        Cleanup();

        return _data.TryRemove(item, out _);
    }

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public void Clear()
    {
        _data.Clear();
    }

    /// <summary>
    /// Removes expired items from the collection.
    /// </summary>
    private void Cleanup()
    {
        var now = _timeProvider.Now;
        var pairs = _data.ToArray();
        foreach (var (item, expires) in pairs)
        {
            if (expires < now)
                _data.TryRemove(item, out _);
        }
    }
}
