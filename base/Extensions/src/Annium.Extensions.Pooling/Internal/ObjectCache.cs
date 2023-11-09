using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Pooling.Internal;

internal sealed class ObjectCache<TKey, TValue> : IObjectCache<TKey, TValue>, ILogSubject
    where TKey : notnull
    where TValue : class
{
    private readonly ObjectCacheProvider<TKey, TValue> _provider;
    public ILogger Logger { get; }
    private readonly ConcurrentDictionary<TKey, CacheEntry> _entries = new();

    public ObjectCache(ObjectCacheProvider<TKey, TValue> provider, ILogger logger)
    {
        _provider = provider;
        Logger = logger;
    }

    public async Task<IDisposableReference<TValue>> GetAsync(TKey key, CancellationToken ct = default)
    {
        this.Trace("start");

        // get or create CacheEntry
        var ctx = new FactoryContext();
        var entry = _entries.GetOrAdd(key, Factory, ctx);
        var isInitializing = ctx.IsCreated;

        var operation = isInitializing ? "new value created" : "existing value used";
        this.Trace<TKey, string>("Get by {key}: {operation}", key, operation);

        // creator - immediately creates value, others - wait for access
        IDisposableReference<TValue>? reference = null;
        if (isInitializing)
        {
            this.Trace("Get by {key}: initialize entry", key);
            var value = await _provider.CreateAsync(key, ct);
            value.Switch(
                x => entry.SetValue(x),
                x =>
                {
                    reference = x;
                    entry.SetValue(x.Value);
                }
            );

            this.Trace("Get by {key}: entry ready", key);
        }
        else
        {
            this.Trace("Get by {key}: wait entry", key);
            await entry.WaitAsync();
        }

        // if not initializing and entry has no references - it is suspended, need to resume
        if (!isInitializing && !entry.HasReferences)
        {
            this.Trace("Get by {key}: resume entry", key);
            await _provider.ResumeAsync(key, entry.Value);
        }

        // create reference, incrementing reference counter
        this.Trace("Get by {key}: add entry reference", key);
        entry.AddReference();
        reference ??= Disposable.Reference(entry.Value, () => Release(key, entry));

        entry.Release();

        return reference;
    }

    private async Task Release(TKey key, CacheEntry entry)
    {
        this.Trace("Release by {key}: wait entry", key);
        await entry.WaitAsync();

        this.Trace("Release by {key}: remove reference", key);
        entry.RemoveReference();
        if (!entry.HasReferences)
        {
            this.Trace("Release by {key}: suspend entry", key);
            await _provider.SuspendAsync(key, entry.Value);
        }

        entry.Release();
    }

    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        var cacheEntries = _entries.ToArray();
        _entries.Clear();

        this.Trace("dispose {count} entries", cacheEntries.Length);

        foreach (var (key, entry) in cacheEntries)
        {
            this.Trace("dispose {entry}", entry);
            entry.Dispose();
            await _provider.DisposeAsync(key, entry.Value);
        }

        this.Trace("done");
    }

    private static CacheEntry Factory(TKey key, FactoryContext ctx)
    {
        ctx.IsCreated = true;

        return new CacheEntry();
    }

    private record FactoryContext
    {
        public bool IsCreated;
    }

    private sealed record CacheEntry : IDisposable
    {
        public TValue Value => _value ?? throw new InvalidOperationException("Value is not set");
        public bool HasReferences => _references != 0;

        private readonly AutoResetEvent _gate = new(initialState: false);
        private TValue? _value;
        private uint _references;

        public Task WaitAsync() => Task.Run(() => _gate.WaitOne());

        public void Release() => _gate.Set();

        public void SetValue(TValue value)
        {
            if (_value is null)
                _value = value;
            else
                throw new InvalidOperationException("Can't change CacheEntry Value");
        }

        public void AddReference() => ++_references;

        public void RemoveReference() => --_references;

        public override string ToString() => $"{Value} [{_references}]";

        public void Dispose()
        {
            _gate.Reset();
            _gate.Dispose();
        }
    }
}
