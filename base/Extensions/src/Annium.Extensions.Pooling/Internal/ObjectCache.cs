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
                try
                {
                    var value = await _provider.CreateAsync(key, ct);
                    value.Switch(
                        x => entry.SetValue(x),
                        x =>
                        {
                            // the provider's own reference belongs to the entry, not to whoever happened to
                            // create it: handing it back as the caller's would leave the release that the
                            // cache counts on unwired, so the entry never dropped to zero references and
                            // was never suspended
                            entry.SetOwnedReference(x);
                            entry.SetValue(x.Value);
                        }
                    );
                }
                catch (Exception e)
                {
                    // factory failure — populate-after-success invariant requires removing the
                    // placeholder so that a subsequent GetAsync(key) triggers a FRESH factory
                    // call. The failure is recorded on the entry and the gate opened: the gate is
                    // a single-permit semaphore, so it wakes exactly one waiter, and that waiter passes the
                    // wake-up along before throwing. Releasing without recording the failure woke
                    // one waiter, which then failed on the unset value and broke the chain, leaving
                    // every other waiter blocked for good.
                    _entries.TryRemove(key, out _);
                    entry.SetFailed(e);
                    throw;
                }

                this.Trace("Get by {key}: entry {entry} ready", key, entry);
            }
            else
            {
                this.Trace("Get by {key}: wait entry {entry}", key, entry);
                if (!await entry.WaitAsync(ct))
                    throw new ObjectDisposedException(
                        nameof(ObjectCache<,>),
                        $"Cache entry for key '{key}' was disposed while waiting for it"
                    );

                if (entry.Error is not null)
                {
                    this.Trace("Get by {key}: entry {entry} failed to initialize", key, entry);
                    // hand the wake-up to the next waiter before leaving
                    entry.Release();

                    throw new InvalidOperationException($"Failed to create value for key '{key}'", entry.Error);
                }
            }

            // if not initializing and entry has no references - it is suspended, need to resume
            if (!isNew && !entry.HasReferences)
            {
                this.Trace("Get by {key}: resume entry {entry}", key, entry);

                try
                {
                    await _provider.ResumeAsync(key, entry.Value);
                }
                catch (Exception)
                {
                    // same reasoning as the suspend path: the gate goes back before the failure travels on
                    entry.Release();

                    throw;
                }
            }

            // create reference, incrementing reference counter
            this.Trace("Get by {key}: add entry {entry} reference", key, entry);
            entry.AddReference();
            reference = Disposable.Reference(
                entry.Value,
                async () => await ReleaseAsync(key, entry).ConfigureAwait(false)
            );

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
            if (!await entry.WaitAsync())
            {
                // the entry is already gone, and with it whatever this reference was holding
                this.Trace("Release by {key}: entry {entry} already disposed", key, entry);

                return;
            }

            try
            {
                this.Trace("Release by {key}: remove reference from entry {entry}", key, entry);
                entry.RemoveReference();
                if (!entry.HasReferences)
                {
                    this.Trace("Release by {key}: suspend entry {entry}", key, entry);
                    await _provider.SuspendAsync(key, entry.Value);
                }
            }
            finally
            {
                // the gate is taken to do this and has to go back whatever happened: a provider that threw
                // while suspending used to keep it, and every later use of the key - the cache's own
                // disposal included - then waited on a gate nobody would hand back
                entry.Release();
            }
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
                // each entry holds a resource of its own, so a failure disposing one must not stop the rest
                // from being disposed
                try
                {
                    this.Trace("await {entry} value", entry);
                    await entry.WaitAsync();
                    this.Trace("dispose {entry}", entry);
                    entry.Dispose();
                }
                catch (Exception e)
                {
                    this.Error(e);
                }

                // the provider's reference and the provider's own teardown release different things, so a
                // failure in one must not skip the other - the same reasoning as the loop around them
                try
                {
                    await entry.DisposeOwnedReferenceAsync();
                }
                catch (Exception e)
                {
                    this.Error(e);
                }

                try
                {
                    await _provider.DisposeAsync(key, entry.Value);
                }
                catch (Exception e)
                {
                    this.Error(e);
                }
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
        /// Gets the failure raised while creating the value, or null when creation did not fail.
        /// </summary>
        public Exception? Error { get; private set; }

        /// <summary>
        /// Synchronization gate for coordinating access to the entry.
        /// </summary>
        private readonly SemaphoreSlim _gate = new(0, 1);

        /// <summary>
        /// Cancelled when the entry is torn down, so everyone waiting for it is told, rather than one of
        /// them being woken as though the entry were theirs to use
        /// </summary>
        private readonly CancellationTokenSource _disposing = new();

        /// <summary>
        /// The cached value.
        /// </summary>
        private TValue? _value;

        /// <summary>
        /// The number of active references to this entry.
        /// </summary>
        private uint _references;

        /// <summary>
        /// The reference the provider returned alongside the value, owned by this entry.
        /// </summary>
        private IDisposableReference<TValue>? _ownedReference;

        /// <summary>
        /// Asynchronously waits for the entry to be ready for access.
        /// </summary>
        /// <param name="ct">Cancellation token for giving up the wait.</param>
        /// <returns>A task that completes when the entry is ready.</returns>
        public async Task<bool> WaitAsync(CancellationToken ct = default)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _disposing.Token);

            try
            {
                await _gate.WaitAsync(linked.Token);

                return true;
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                // the entry was torn down while this call waited for it: not the caller's cancellation, and
                // not an acquisition either - which of the two it was is what the return value carries
                return false;
            }
            catch (ObjectDisposedException)
            {
                // teardown landed between the check and the wait
                return false;
            }
        }

        /// <summary>
        /// Signals that the entry is ready for access.
        /// </summary>
        public void Release()
        {
            if (_disposing.IsCancellationRequested)
                return;

            try
            {
                _gate.Release();
            }
            catch (ObjectDisposedException)
            {
                // teardown got between the check and the release; nobody is left to hand it to
            }
            catch (SemaphoreFullException)
            {
                // not expected: the creator hands over exactly one permit and every acquire is matched by
                // one release. Caught so that a slip in that would not mask the failure this often runs
                // alongside, in the finally of a path that is already throwing
            }
        }

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
        /// Records that creating the value failed, and wakes a waiter so the failure travels down the
        /// chain instead of leaving everyone blocked.
        /// </summary>
        /// <param name="error">The failure raised by the factory.</param>
        public void SetFailed(Exception error)
        {
            Error = error;
            Release();
        }

        /// <summary>
        /// Takes ownership of the reference the provider returned alongside the value, so that it is
        /// released when the entry goes rather than when whoever created it lets go.
        /// </summary>
        /// <param name="reference">The provider's reference to the value.</param>
        public void SetOwnedReference(IDisposableReference<TValue> reference) => _ownedReference = reference;

        /// <summary>
        /// Releases the provider's reference, if it gave one.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async ValueTask DisposeOwnedReferenceAsync()
        {
            if (_ownedReference is null)
                return;

            await _ownedReference.DisposeAsync();
            _ownedReference = null;
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
            // cancelled before anything is disposed: disposing the semaphore does not wake what waits on
            // it, and a bare release would wake exactly one waiter as though the entry were still theirs to
            // use. Cancelling tells every one of them, and tells them which it was
            _disposing.Cancel();
            _gate.Dispose();
            _disposing.Dispose();
        }
    }
}
