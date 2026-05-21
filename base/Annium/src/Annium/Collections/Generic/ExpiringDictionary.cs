using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Internal.Collections.Generic;
using NodaTime;

namespace Annium.Collections.Generic;

/// <summary>
/// Represents a dictionary of key-value pairs that expire after a specified duration. Reads check
/// per-item expiry on every call; a background timer periodically evicts stale entries.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this facade exists:</b> all storage and eviction is implemented by the internal
/// <see cref="ExpiringStore{TKey,TValue}"/>. This type is a thin public surface that
/// (a) keeps <see cref="ExpiringStore{TKey, TValue}"/> internal so its representation can change
/// without breaking the public surface, and (b) presents the expected dictionary-shaped API
/// (<see cref="Get"/> throws <see cref="KeyNotFoundException"/> rather than returning a status flag,
/// matching <c>Dictionary&lt;TKey, TValue&gt;</c> ergonomics). The one-line delegations are deliberate
/// — every method is a direct mapping to the corresponding store method, with the sole exception of
/// <see cref="Get"/> which adds the throw-on-missing semantics.
/// </para>
/// <para>
/// <b>Lifetime obligation:</b> instances own a <see cref="System.Threading.Timer"/> for background
/// eviction and therefore implement <see cref="IDisposable"/>. Callers MUST dispose the instance when
/// done; failing to do so leaks the timer (no finalizer is provided) and the eviction tick will keep
/// firing until garbage collection eventually reclaims the underlying timer queue entry, potentially
/// after a significant delay in long-lived applications.
/// </para>
/// </remarks>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
public sealed class ExpiringDictionary<TKey, TValue> : IDisposable, IAsyncDisposable
    where TKey : notnull
{
    /// <summary>The underlying expiring store that holds the entries and evicts them on expiry.</summary>
    private readonly ExpiringStore<TKey, TValue> _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiringDictionary{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="timeProvider">The time provider to use for determining expiration times.</param>
    public ExpiringDictionary(ITimeProvider timeProvider)
        : this(timeProvider, ExpiringStore<TKey, TValue>.DefaultEvictionInterval) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiringDictionary{TKey, TValue}"/> class with a custom eviction interval.
    /// </summary>
    /// <param name="timeProvider">The time provider to use for determining expiration times.</param>
    /// <param name="evictionInterval">How often the background eviction pass runs.</param>
    public ExpiringDictionary(ITimeProvider timeProvider, TimeSpan evictionInterval)
    {
        _store = new ExpiringStore<TKey, TValue>(timeProvider, evictionInterval);
    }

    /// <summary>
    /// Adds a key-value pair to the dictionary with the specified time-to-live duration.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <param name="ttl">The duration after which the element will expire.</param>
    public void Add(TKey key, TValue value, Duration ttl)
    {
        _store.Add(key, value, ttl);
    }

    /// <summary>
    /// Attempts to get the value associated with the specified key. Returns false when the key is
    /// absent OR the entry has already expired.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">When this method returns, contains the value if the key exists and the entry has not expired; otherwise default.</param>
    /// <returns><see langword="true"/> when the key is present and non-expired; otherwise <see langword="false"/>.</returns>
    public bool TryGet(TKey key, out TValue value)
    {
        return _store.TryGet(key, out value);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The value associated with the key.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the key is missing or its entry has expired.</exception>
    public TValue Get(TKey key)
    {
        if (_store.TryGet(key, out var value))
            return value;

        throw new KeyNotFoundException($"Key {key} is missing in collection");
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key with a non-expired entry.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns><see langword="true"/> when the key is present and non-expired; otherwise <see langword="false"/>.</returns>
    public bool ContainsKey(TKey key)
    {
        return _store.ContainsKey(key);
    }

    /// <summary>
    /// Removes the value with the specified key from the dictionary.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> ONLY when the key was present AND non-expired at the time of removal. An
    /// expired entry is still PHYSICALLY removed from the dictionary, but the method returns <c>false</c>
    /// with <paramref name="value"/> set to <c>default</c>. Callers that need to distinguish "key was
    /// absent" from "key was expired" must check expiry separately via <see cref="ContainsKey"/> or
    /// <see cref="TryGet"/> beforehand.
    /// </remarks>
    /// <param name="key">The key to remove.</param>
    /// <param name="value">When this method returns, contains the removed value if the key was present and non-expired; otherwise default.</param>
    /// <returns><see langword="true"/> when the key was present and non-expired at the time of removal; otherwise <see langword="false"/>.</returns>
    public bool Remove(TKey key, out TValue value)
    {
        return _store.Remove(key, out value);
    }

    /// <summary>
    /// Removes all keys and values from the dictionary.
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
