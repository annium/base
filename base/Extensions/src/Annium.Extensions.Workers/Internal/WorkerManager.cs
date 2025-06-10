using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Execution.Background;
using Annium.Logging;

namespace Annium.Extensions.Workers.Internal;

/// <summary>
/// Internal implementation of a worker manager that handles lifecycle management of keyed workers
/// </summary>
/// <typeparam name="TKey">The type of key used to identify workers</typeparam>
internal sealed class WorkerManager<TKey> : IWorkerManager<TKey>, IAsyncDisposable, ILogSubject
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Gets the logger instance for this worker manager
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Dictionary containing all managed worker entries indexed by key
    /// </summary>
    private readonly Dictionary<TKey, Entry> _entries = new();

    /// <summary>
    /// Service provider for resolving worker dependencies
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// Background executor for managing worker lifecycle operations
    /// </summary>
    private readonly IExecutor _executor;

    /// <summary>
    /// Flag indicating whether this manager has been disposed
    /// </summary>
    private bool _isDisposed;

    public WorkerManager(IServiceProvider sp, ILogger logger)
    {
        Logger = logger;
        _sp = sp;
        _executor = Executor.Concurrent<WorkerManager<TKey>>(logger).Start();
    }

    /// <summary>
    /// Starts a worker for the specified key, creating it if it doesn't exist
    /// </summary>
    /// <param name="key">The key identifying the worker to start</param>
    /// <returns>A task that completes when the worker is started</returns>
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
                _entries[key] = entry = new Entry(_sp.Resolve<WorkerBase<TKey>>());
                _executor.Schedule(async () =>
                {
                    this.Trace("await init of entry {entry} for {key}", entry.GetFullId(), key);
                    await entry.WorkerBase.InitAsync(key);

                    this.Trace("mark started entry {entry} for {key}", entry.GetFullId(), key);
                    entry.SetStarted();
                });
            }
        }

        this.Trace("await start of entry {entry} for {key}", entry.GetFullId(), key);
        await entry.WhenStarted;

        this.Trace("done for {key}", key);
    }

    /// <summary>
    /// Stops and disposes the worker for the specified key
    /// </summary>
    /// <param name="key">The key identifying the worker to stop</param>
    /// <returns>A task that completes when the worker is stopped</returns>
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
                    await entry.WorkerBase.DisposeAsync();

                    this.Trace("remove entry of entry {entry} for {key}", entry.GetFullId(), key);
                    lock (_entries)
                        _entries.Remove(key);

                    this.Trace("mark stopped entry of entry {entry} for {key}", entry.GetFullId(), key);
                    entry.SetStopped();
                });
            }
        }

        this.Trace("await stop of entry {entry} for {key}", entry.GetFullId(), key);
#pragma warning disable VSTHRD003
        await entry.WhenStopped;
#pragma warning restore VSTHRD003

        this.Trace("done for {key}", key);
    }

    /// <summary>
    /// Disposes the worker manager and all its managed workers
    /// </summary>
    /// <returns>A task that completes when disposal is finished</returns>
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

    /// <summary>
    /// Ensures that the worker manager has not been disposed
    /// </summary>
    private void EnsureIsNotDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(WorkerManager<TKey>));
    }

    /// <summary>
    /// Represents a managed worker entry with lifecycle tracking
    /// </summary>
    /// <param name="WorkerBase">The worker instance being managed</param>
    private record Entry(WorkerBase<TKey> WorkerBase)
    {
        /// <summary>
        /// Gets a task that completes when the worker is started
        /// </summary>
        public Task WhenStarted => _startedTcs.Task;

        /// <summary>
        /// Gets a value indicating whether the worker is currently stopping
        /// </summary>
        public bool IsStopping { get; private set; }

        /// <summary>
        /// Gets a task that completes when the worker is stopped
        /// </summary>
        public Task WhenStopped => _stoppedTcs.Task;

        /// <summary>
        /// Task completion source for tracking worker start completion
        /// </summary>
        private readonly TaskCompletionSource _startedTcs = new();

        /// <summary>
        /// Task completion source for tracking worker stop completion
        /// </summary>
        private readonly TaskCompletionSource _stoppedTcs = new();

        /// <summary>
        /// Marks the worker as started by completing the start task
        /// </summary>
        public void SetStarted() => _startedTcs.SetResult();

        /// <summary>
        /// Marks the worker as currently stopping
        /// </summary>
        public void SetIsStopping() => IsStopping = true;

        /// <summary>
        /// Marks the worker as stopped by completing the stop task
        /// </summary>
        public void SetStopped() => _stoppedTcs.SetResult();
    }
}
