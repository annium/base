using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Pooling.Internal;

/// <summary>
/// Thread-safe object cache that manages keyed objects with automatic lifecycle management.
/// Supports create, suspend, resume, and dispose operations through a provider pattern.
/// </summary>
/// <typeparam name="TKey">The type of keys used to identify cached objects. Must be non-null.</typeparam>
/// <typeparam name="TValue">The type of values stored in the cache. Must be a reference type.</typeparam>
internal sealed class ObjectCache<TKey, TValue> : IObjectCache<TKey, TValue>, ILogSubject
    where TKey : notnull
    where TValue : class
{
    /// <summary>
    /// Provider responsible for object lifecycle operations.
    /// </summary>
    private readonly ObjectCacheProvider<TKey, TValue> _provider;

    /// <summary>
    /// Gets the logger instance for this cache.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Thread-safe dictionary storing cache entries by key.
    /// </summary>
    private readonly ConcurrentDictionary<TKey, CacheEntry> _entries = new();

    /// <summary>
    /// Initializes a new instance of the ObjectCache class.
    /// </summary>
    /// <param name="provider">Provider responsible for object lifecycle operations.</param>
    /// <param name="logger">Logger instance for cache operations.</param>
    public ObjectCache(ObjectCacheProvider<TKey, TValue> provider, ILogger logger)
    {
        _provider = provider;
        Logger = logger;
    }

    /// <summary>
    /// Gets or creates a cached object for the specified key and returns a disposable reference to it.
    /// The object will be suspended when all references are released.
    /// </summary>
    /// <param name="key">The key identifying the cached object.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A disposable reference to the cached object.</returns>
    public async Task<IDisposableReference<TValue>> GetAsync(TKey key, CancellationToken ct = default)
    {
        try
        {
            this.Trace("start");

            // get or create CacheEntry
            var ctx = new FactoryContext();
            var entry = _entries.GetOrAdd(key, Factory, ctx);
            var isNew = ReferenceEquals(ctx.Entry, entry);

            var operation = isNew ? "new entry created" : "existing entry used";
            this.Trace("Get by {key}: {operation} {entry}", key, operation, entry);

            // creator - immediately creates value, others - wait for access
            IDisposableReference<TValue>? reference = null;
            if (isNew)
            {
                this.Trace("Get by {key}: initialize entry {entry}", key, entry);
                var value = await _provider.CreateAsync(key, ct);
                value.Switch(
                    x => entry.SetValue(x),
                    x =>
                    {
                        reference = x;
                        entry.SetValue(x.Value);
                    }
                );

                this.Trace("Get by {key}: entry {entry} ready", key, entry);
            }
            else
            {
                this.Trace("Get by {key}: wait entry {entry}", key, entry);
                await entry.WaitAsync();
            }

            // if not initializing and entry has no references - it is suspended, need to resume
            if (!isNew && !entry.HasReferences)
            {
                this.Trace("Get by {key}: resume entry {entry}", key, entry);
                await _provider.ResumeAsync(key, entry.Value);
            }

            // create reference, incrementing reference counter
            this.Trace("Get by {key}: add entry {entry} reference", key, entry);
            entry.AddReference();
            reference ??= Disposable.Reference(entry.Value, () => ReleaseAsync(key, entry));

            entry.Release();

            return reference;
        }
        catch (Exception e)
        {
            this.Error(e);
            throw;
        }
    }

    /// <summary>
    /// Releases a reference to a cached object and suspends it if no more references exist.
    /// </summary>
    /// <param name="key">The key identifying the cached object.</param>
    /// <param name="entry">The cache entry containing the object.</param>
    /// <returns>A task that represents the asynchronous release operation.</returns>
    private async Task ReleaseAsync(TKey key, CacheEntry entry)
    {
        try
        {
            this.Trace("Release by {key}: wait entry {entry}", key, entry);
            await entry.WaitAsync();

            this.Trace("Release by {key}: remove reference from entry {entry}", key, entry);
            entry.RemoveReference();
            if (!entry.HasReferences)
            {
                this.Trace("Release by {key}: suspend entry {entry}", key, entry);
                await _provider.SuspendAsync(key, entry.Value);
            }

            entry.Release();
        }
        catch (Exception e)
        {
            this.Error(e);
        }
    }

    /// <summary>
    /// Asynchronously disposes all cached objects and clears the cache.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        try
        {
            this.Trace("start");

            var cacheEntries = _entries.ToArray();
            _entries.Clear();

            this.Trace("dispose {count} entries", cacheEntries.Length);

            foreach (var (key, entry) in cacheEntries)
            {
                this.Trace("await {entry} value", entry);
                await entry.WaitAsync();
                this.Trace("dispose {entry}", entry);
                entry.Dispose();
                await _provider.DisposeAsync(key, entry.Value);
            }

            this.Trace("done");
        }
        catch (Exception e)
        {
            this.Error(e);
        }
    }

    /// <summary>
    /// Factory method for creating new cache entries.
    /// </summary>
    /// <param name="key">The key for the cache entry.</param>
    /// <param name="ctx">Factory context containing the created entry.</param>
    /// <returns>A new cache entry.</returns>
    private static CacheEntry Factory(TKey key, FactoryContext ctx) => ctx.Entry = new CacheEntry();

    /// <summary>
    /// Context object used by the factory method to store the created cache entry.
    /// </summary>
    private record FactoryContext
    {
        /// <summary>
        /// The cache entry created by the factory.
        /// </summary>
        public CacheEntry? Entry;
    }

    /// <summary>
    /// Represents a cache entry that manages access to a cached value with reference counting and synchronization.
    /// </summary>
    private sealed record CacheEntry : IDisposable
    {
        /// <summary>
        /// Gets the cached value. Throws if the value has not been set.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the value is not set.</exception>
        public TValue Value => _value ?? throw new InvalidOperationException("Value is not set");

        /// <summary>
        /// Gets a value indicating whether this entry has any active references.
        /// </summary>
        public bool HasReferences => _references != 0;

        /// <summary>
        /// Synchronization gate for coordinating access to the entry.
        /// </summary>
        private readonly AutoResetEvent _gate = new(initialState: false);

        /// <summary>
        /// The cached value.
        /// </summary>
        private TValue? _value;

        /// <summary>
        /// The number of active references to this entry.
        /// </summary>
        private uint _references;

        /// <summary>
        /// Asynchronously waits for the entry to be ready for access.
        /// </summary>
        /// <returns>A task that completes when the entry is ready.</returns>
        public Task WaitAsync() => Task.Run(() => _gate.WaitOne());

        /// <summary>
        /// Signals that the entry is ready for access.
        /// </summary>
        public void Release() => _gate.Set();

        /// <summary>
        /// Sets the cached value. Can only be called once.
        /// </summary>
        /// <param name="value">The value to cache.</param>
        /// <exception cref="InvalidOperationException">Thrown when attempting to change an already set value.</exception>
        public void SetValue(TValue value)
        {
            if (_value is null)
                _value = value;
            else
                throw new InvalidOperationException("Can't change CacheEntry Value");
        }

        /// <summary>
        /// Increments the reference count for this entry.
        /// </summary>
        public void AddReference() => ++_references;

        /// <summary>
        /// Decrements the reference count for this entry.
        /// </summary>
        public void RemoveReference() => --_references;

        /// <summary>
        /// Returns a string representation of the cache entry including its value and reference count.
        /// </summary>
        /// <returns>A string representation of the cache entry.</returns>
        public override string ToString() => $"{this.GetFullId()} {_value?.ToString() ?? "null"} [{_references}]";

        /// <summary>
        /// Releases all resources used by the cache entry.
        /// </summary>
        public void Dispose()
        {
            _gate.Reset();
            _gate.Dispose();
        }
    }
}
