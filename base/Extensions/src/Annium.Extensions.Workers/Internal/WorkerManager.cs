using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Execution.Background;
using Annium.Logging;

namespace Annium.Extensions.Workers.Internal;

internal sealed class WorkerManager<TKey> : IWorkerManager<TKey>, IAsyncDisposable, ILogSubject
    where TKey : IEquatable<TKey>
{
    public ILogger Logger { get; }
    private readonly Dictionary<TKey, Entry> _entries = new();
    private readonly IServiceProvider _sp;
    private readonly IExecutor _executor;
    private bool _isDisposed;

    public WorkerManager(IServiceProvider sp, ILogger logger)
    {
        Logger = logger;
        _sp = sp;
        _executor = Executor.Concurrent<WorkerManager<TKey>>(logger).Start();
    }

    public async Task StartAsync(TKey key)
    {
        this.Trace("start for {key}", key);

        EnsureIsNotDisposed();

        Entry? entry;
        lock (_entries)
        {
            if (_entries.TryGetValue(key, out entry))
            {
                this.Trace("skip, already created entry {entry} for {key}", entry.GetFullId(), key);
            }
            else
            {
                this.Trace("create and schedule init of entry {entry} for {key}", entry.GetFullId(), key);
                _entries[key] = entry = new Entry(_sp.Resolve<IWorker<TKey>>());
                _executor.Schedule(async () =>
                {
                    this.Trace("await init of entry {entry} for {key}", entry.GetFullId(), key);
                    await entry.Worker.InitAsync(key);

                    this.Trace("mark started entry {entry} for {key}", entry.GetFullId(), key);
                    entry.SetStarted();
                });
            }
        }

        this.Trace("await start of entry {entry} for {key}", entry.GetFullId(), key);
        await entry.WhenStarted;

        this.Trace("done for {key}", key);
    }

    public async Task StopAsync(TKey key)
    {
        this.Trace("start for {key}", key);

        EnsureIsNotDisposed();

        Entry? entry;
        lock (_entries)
        {
            if (!_entries.TryGetValue(key, out entry))
            {
                this.Trace("skip, entry for {key} not found", key);
                return;
            }

            if (entry.IsStopping)
                this.Trace("already stopping entry {entry} for {key}", entry.GetFullId(), key);
            else
            {
                this.Trace("schedule disposal of entry {entry} for {key}", entry.GetFullId(), key);
                entry.SetIsStopping();
                _executor.Schedule(async () =>
                {
                    this.Trace("await disposal of entry {entry} for {key}", entry.GetFullId(), key);
                    await entry.Worker.DisposeAsync();

                    this.Trace("remove entry of entry {entry} for {key}", entry.GetFullId(), key);
                    lock (_entries)
                        _entries.Remove(key);

                    this.Trace("mark stopped entry of entry {entry} for {key}", entry.GetFullId(), key);
                    entry.SetStopped();
                });
            }
        }

        this.Trace("await stop of entry {entry} for {key}", entry.GetFullId(), key);
        await entry.WhenStopped;

        this.Trace("done for {key}", key);
    }

    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        _isDisposed = true;

        this.Trace("await workers");
        await _executor.DisposeAsync();

        this.Trace("clear entries");
        lock (_entries)
            _entries.Clear();

        this.Trace("done");
    }

    private void EnsureIsNotDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(WorkerManager<TKey>));
    }

    private record Entry(IWorker<TKey> Worker)
    {
        public Task WhenStarted => _startedTcs.Task;
        public bool IsStopping { get; private set; }
        public Task WhenStopped => _stoppedTcs.Task;
        private readonly TaskCompletionSource _startedTcs = new();
        private readonly TaskCompletionSource _stoppedTcs = new();

        public void SetStarted() => _startedTcs.SetResult();

        public void SetIsStopping() => IsStopping = true;

        public void SetStopped() => _stoppedTcs.SetResult();
    }
}
