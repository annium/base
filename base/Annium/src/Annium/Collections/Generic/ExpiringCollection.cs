using System;
using NodaTime;

namespace Annium.Collections.Generic;

/// <summary>
/// Represents a collection of items that expire after a specified duration. Reads check per-item
/// expiry on every call; a background timer periodically evicts stale entries to bound memory growth.
/// </summary>
/// <remarks>
/// <para>
/// <b>Lifetime obligation:</b> instances own a <see cref="System.Threading.Timer"/> for background
/// eviction and therefore implement <see cref="IDisposable"/>. Callers MUST dispose the instance when
/// done; failing to do so leaks the timer (no finalizer is provided) and the eviction tick will keep
/// firing until garbage collection eventually reclaims the underlying timer queue entry, potentially
/// after a significant delay in long-lived applications.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of the items in the collection.</typeparam>
public sealed class ExpiringCollection<T> : IDisposable
    where T : notnull
{
    private readonly ExpiringStore<T, byte> _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiringCollection{T}"/> class with the specified time provider.
    /// </summary>
    /// <param name="timeProvider">The time provider to use for determining expiration times.</param>
    public ExpiringCollection(ITimeProvider timeProvider)
        : this(timeProvider, ExpiringStore<T, byte>.DefaultEvictionInterval) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiringCollection{T}"/> class with the specified time provider and eviction interval.
    /// </summary>
    /// <param name="timeProvider">The time provider to use for determining expiration times.</param>
    /// <param name="evictionInterval">How often the background eviction pass runs.</param>
    public ExpiringCollection(ITimeProvider timeProvider, TimeSpan evictionInterval)
    {
        _store = new ExpiringStore<T, byte>(timeProvider, evictionInterval);
    }

    /// <summary>
    /// Adds an item to the collection with the specified time-to-live duration.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="ttl">The duration after which the item will expire.</param>
    public void Add(T item, Duration ttl)
    {
        _store.Add(item, default, ttl);
    }

    /// <summary>
    /// Checks if the collection contains the specified item and that it has not expired.
    /// </summary>
    /// <param name="item">The item to check for.</param>
    /// <returns>True if the item is present and not expired; otherwise, false.</returns>
    public bool Contains(T item)
    {
        return _store.ContainsKey(item);
    }

    /// <summary>
    /// Removes the specified item from the collection.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was successfully removed; otherwise, false.</returns>
    public bool Remove(T item)
    {
        return _store.Remove(item, out _);
    }

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    public void Clear()
    {
        _store.Clear();
    }

    /// <summary>
    /// Stops the background eviction timer and releases resources.
    /// </summary>
    public void Dispose()
    {
        _store.Dispose();
    }
}
