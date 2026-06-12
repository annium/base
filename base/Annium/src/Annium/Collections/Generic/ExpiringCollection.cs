using System;
using System.Threading.Tasks;
using Annium.Internal.Collections.Generic;
using NodaTime;

namespace Annium.Collections.Generic;

/// <summary>
/// Represents a collection of items that expire after a specified duration. Reads check per-item
/// expiry on every call; a background timer periodically evicts stale entries to bound memory growth.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this facade exists:</b> all storage and eviction is implemented by the internal
/// <see cref="ExpiringStore{TKey,TValue}"/>. This type is a thin Set-shaped public surface that
/// (a) keeps <see cref="ExpiringStore{TKey, TValue}"/> internal so its <see cref="byte"/>-as-filler-value
/// representation is not exposed to consumers, (b) presents Set semantics (<see cref="Add"/> /
/// <see cref="Contains"/> / <see cref="Remove"/>) instead of the dictionary-shaped store API, and
/// (c) provides a stable public type whose underlying representation can change without breaking
/// the public surface. The one-line delegations are deliberate — every method is a direct mapping
/// to the corresponding store method.
/// </para>
/// <para>
/// <b>Lifetime obligation:</b> instances own a <see cref="System.Threading.Timer"/> for background
/// eviction and therefore implement <see cref="IDisposable"/>. Callers MUST dispose the instance when
/// done; failing to do so leaks the timer (no finalizer is provided) and the eviction tick will keep
/// firing until garbage collection eventually reclaims the underlying timer queue entry, potentially
/// after a significant delay in long-lived applications.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of the items in the collection.</typeparam>
public sealed class ExpiringCollection<T> : IDisposable, IAsyncDisposable
    where T : notnull
{
    /// <summary>The underlying expiring store that holds entries and evicts them on expiry.</summary>
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
    /// <remarks>
    /// Returns <c>true</c> ONLY when the item was present AND non-expired at the time of removal. An
    /// expired item is still PHYSICALLY removed from the collection, but the method returns <c>false</c>
    /// — callers that need to distinguish "item was absent" from "item was expired" must check expiry
    /// separately via <see cref="Contains"/> beforehand.
    /// </remarks>
    /// <param name="item">The item to remove.</param>
    /// <returns>True if the item was present and non-expired; otherwise, false.</returns>
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
    public void Dispose() => _store.Dispose();

    /// <summary>
    /// Asynchronously stops the background eviction timer and releases resources. The drain is
    /// currently synchronous; this method exists to satisfy <see cref="IAsyncDisposable"/> for callers
    /// that prefer <c>await using</c>.
    /// </summary>
    /// <returns>A completed <see cref="ValueTask"/>.</returns>
    public ValueTask DisposeAsync() => _store.DisposeAsync();
}
