using System;
using System.Collections.Concurrent;
using System.Threading;
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
internal sealed class ExpiringStore<TKey, TValue> : IDisposable
    where TKey : notnull
{
    /// <summary>
    /// The default interval between background eviction passes.
    /// </summary>
    internal static readonly TimeSpan DefaultEvictionInterval = TimeSpan.FromMinutes(1);

    private readonly ITimeProvider _timeProvider;
    private readonly ConcurrentDictionary<TKey, Entry> _data = new();
    private readonly Timer _evictionTimer;
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
            static state => ((ExpiringStore<TKey, TValue>)state!).Evict(),
            this,
            evictionInterval,
            evictionInterval
        );
    }

    /// <summary>
    /// Adds or updates an entry with the specified key, value, and time-to-live.
    /// </summary>
    public void Add(TKey key, TValue value, Duration ttl)
    {
        var entry = new Entry(value, _timeProvider.Now + ttl);
        _data.AddOrUpdate(key, entry, (_, _) => entry);
    }

    /// <summary>
    /// Tests whether the store contains a non-expired entry for the specified key.
    /// </summary>
    public bool ContainsKey(TKey key)
    {
        return _data.TryGetValue(key, out var entry) && entry.Expires > _timeProvider.Now;
    }

    /// <summary>
    /// Attempts to retrieve the value for the specified key. Returns false if the entry is missing
    /// or has already expired.
    /// </summary>
    public bool TryGet(TKey key, out TValue value)
    {
        if (_data.TryGetValue(key, out var entry) && entry.Expires > _timeProvider.Now)
        {
            value = entry.Value;
            return true;
        }

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
    public bool Remove(TKey key, out TValue value)
    {
        if (_data.TryRemove(key, out var entry) && entry.Expires > _timeProvider.Now)
        {
            value = entry.Value;
            return true;
        }

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
        // Evict() is a small, fast pass over the dictionary; a tight 1s budget is plenty.
        if (drained.WaitOne(TimeSpan.FromSeconds(1)))
        {
            drained.Dispose();
            return;
        }
        // Drain timed out: in-flight Evict() may still call WaitHandle.Set() after we return. Disposing the
        // handle now would crash the ThreadPool thread; leak it so the late Set is harmless.
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
    private sealed record Entry(TValue Value, Instant Expires);
}
