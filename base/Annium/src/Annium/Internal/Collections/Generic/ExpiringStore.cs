using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;

namespace Annium.Internal.Collections.Generic;

/// <summary>
/// A thread-safe key/value store with TTL-based expiry. Reads check per-item expiry on every call so that
/// callers never observe a stale entry. Stale entries are also pruned periodically by a background timer
/// to bound memory growth; without the periodic prune, callers would still get correct results but the
/// underlying dictionary would accumulate unreachable entries.
/// </summary>
/// <typeparam name="TKey">The type of the keys.</typeparam>
/// <typeparam name="TValue">The type of the values.</typeparam>
internal sealed class ExpiringStore<TKey, TValue> : IDisposable, IAsyncDisposable
    where TKey : notnull
{
    /// <summary>
    /// The default interval between background eviction passes.
    /// </summary>
    internal static readonly TimeSpan DefaultEvictionInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// The drain budget for <see cref="Dispose"/> when waiting for an in-flight <see cref="Evict"/> callback
    /// to complete. Evict is a tight pass over the dictionary; 1s is plenty.
    /// </summary>
    internal static readonly TimeSpan EvictionDrainTimeout = TimeSpan.FromSeconds(1);

    /// <summary>The time provider used to determine the current instant when checking entry expiry.</summary>
    private readonly ITimeProvider _timeProvider;

    /// <summary>The underlying concurrent dictionary holding the entries.</summary>
    private readonly ConcurrentDictionary<TKey, Entry> _data = new();

    /// <summary>The background timer that periodically evicts expired entries.</summary>
    private readonly Timer _evictionTimer;

    /// <summary>Set to 1 once <see cref="Dispose"/> has run; guards re-entrant disposal.</summary>
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiringStore{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="timeProvider">The time provider.</param>
    /// <param name="evictionInterval">How often the background timer scans for expired entries to remove.</param>
    public ExpiringStore(ITimeProvider timeProvider, TimeSpan evictionInterval)
    {
        _timeProvider = timeProvider;
        _evictionTimer = new Timer(
            // state is the `this` reference passed as the next argument, so the cast target is non-null.
            static state =>
                ((ExpiringStore<TKey, TValue>)state!).Evict(),
            this,
            evictionInterval,
            evictionInterval
        );
    }

    /// <summary>
    /// Adds or updates an entry with the specified key, value, and time-to-live.
    /// </summary>
    /// <param name="key">The key to add or update.</param>
    /// <param name="value">The value to associate with the key.</param>
    /// <param name="ttl">The duration after which the entry expires.</param>
    public void Add(TKey key, TValue value, Duration ttl)
    {
        var entry = new Entry(value, _timeProvider.Now + ttl);
        _data.AddOrUpdate(key, entry, (_, _) => entry);
    }

    /// <summary>
    /// Tests whether the store contains a non-expired entry for the specified key.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns><see langword="true"/> if the key exists and the entry has not expired; otherwise <see langword="false"/>.</returns>
    public bool ContainsKey(TKey key)
    {
        return _data.TryGetValue(key, out var entry) && entry.Expires > _timeProvider.Now;
    }

    /// <summary>
    /// Attempts to retrieve the value for the specified key. Returns false if the entry is missing
    /// or has already expired.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">When this method returns, contains the value if found and non-expired; otherwise default.</param>
    /// <returns><see langword="true"/> when the key is present and non-expired; otherwise <see langword="false"/>.</returns>
    public bool TryGet(TKey key, out TValue value)
    {
        if (_data.TryGetValue(key, out var entry) && entry.Expires > _timeProvider.Now)
        {
            value = entry.Value;
            return true;
        }

        // TryGet contract: caller must check return value before using; default is safe here.
        value = default!;
        return false;
    }

    /// <summary>
    /// Removes the entry with the specified key from the underlying dictionary.
    /// </summary>
    /// <remarks>
    /// Returns <c>true</c> ONLY when an entry existed AND was non-expired at the time of removal. An
    /// expired entry is still PHYSICALLY removed from the dictionary (the eviction would have happened
    /// on the next prune anyway), but the method returns <c>false</c> with <paramref name="value"/> set
    /// to <c>default</c>. Callers that need to distinguish "key was absent" from "key was expired" must
    /// check expiry separately via <see cref="ContainsKey"/> or <see cref="TryGet"/> beforehand.
    /// </remarks>
    /// <param name="key">The key to remove.</param>
    /// <param name="value">When this method returns, contains the removed value if the key was present and non-expired; otherwise default.</param>
    /// <returns><see langword="true"/> when the key was present and non-expired at the time of removal; otherwise <see langword="false"/>.</returns>
    public bool Remove(TKey key, out TValue value)
    {
        if (_data.TryRemove(key, out var entry) && entry.Expires > _timeProvider.Now)
        {
            value = entry.Value;
            return true;
        }

        // Remove contract: caller must check return value before using; default is safe here.
        value = default!;
        return false;
    }

    /// <summary>
    /// Removes all entries.
    /// </summary>
    public void Clear()
    {
        _data.Clear();
    }

    /// <summary>
    /// Stops the background eviction timer and waits for any in-flight <see cref="Evict"/> pass to complete
    /// (bounded by a small drain budget) so callers do not race with a still-running prune. On timeout the
    /// wait handle is intentionally leaked — disposing it while <see cref="Evict"/> still races toward
    /// <see cref="Timer"/>'s internal Set call would surface <see cref="ObjectDisposedException"/> on a
    /// ThreadPool thread (process crash). The dispose path is idempotent: a second call returns
    /// immediately. After <c>Dispose()</c> returns, calls to other methods still operate on the dictionary
    /// (no <see cref="ObjectDisposedException"/>); they simply lack background eviction. This is intentional
    /// for an internal helper consumed by <see cref="Annium.Collections.Generic.ExpiringCollection{T}"/> and
    /// <see cref="Annium.Collections.Generic.ExpiringDictionary{TKey,TValue}"/>.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        var drained = new ManualResetEvent(false);
        _evictionTimer.Dispose(drained);
        if (drained.WaitOne(EvictionDrainTimeout))
        {
            drained.Dispose();
            return;
        }
        // Drain timed out: in-flight Evict() may still call WaitHandle.Set() after we return. Disposing the
        // handle now would crash the ThreadPool thread; leak it so the late Set is harmless.
    }

    /// <summary>
    /// Asynchronously stops the background eviction timer and releases resources. The drain is
    /// synchronous; this method exists so callers consuming this store via <see cref="IAsyncDisposable"/>
    /// (e.g. <see cref="Annium.Collections.Generic.ExpiringCollection{T}"/> behind <c>await using</c>) need
    /// not duplicate the <c>Dispose() + ValueTask.CompletedTask</c> shim.
    /// </summary>
    /// <returns>A completed <see cref="ValueTask"/>.</returns>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Removes entries whose expiration is at or before the current time.
    /// </summary>
    private void Evict()
    {
        var now = _timeProvider.Now;
        foreach (var (key, entry) in _data)
        {
            if (entry.Expires <= now)
                _data.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// An entry stored in the dictionary, pairing the value with its expiration instant.
    /// </summary>
    /// <param name="Value">The stored value.</param>
    /// <param name="Expires">The instant at which the value stops being served.</param>
    private sealed record Entry(TValue Value, Instant Expires);
}
