using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerManager{TKey}"/> class.
    /// </summary>
    /// <param name="sp">Service provider used to resolve per-key worker scopes.</param>
    /// <param name="logger">Logger for tracing worker lifecycle.</param>
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

        while (true)
        {
            // re-checked each time round: this loop awaits a pending stop, and the manager can be disposed
            // while it does. Creating an entry after that leaves the caller waiting on a worker that the
            // disposed executor will never run
            EnsureIsNotDisposed();

            Entry? entry;
            Task? pendingStop = null;
            lock (_entries)
            {
                if (_entries.TryGetValue(key, out entry))
                {
                    // an entry on its way out is not something to hand back as started: the caller would be
                    // told its worker is running while that worker is being torn down. Wait for the stop to
                    // finish, then build a fresh one
                    if (entry.IsStopping)
                    {
                        this.Trace("await stop of entry {entry} for {key}", entry.GetFullId(), key);
                        pendingStop = entry.WhenStopped;
                    }
                    else
                        this.Trace("skip, already created entry {entry} for {key}", entry.GetFullId(), key);
                }
                else
                {
                    _entries[key] = entry = new Entry(_sp.Resolve<WorkerBase<TKey>>());
                    // traced after the assignment: reading `entry` before it exists logged a null id for
                    // the one case this line is meant to make visible
                    this.Trace("create and schedule init of entry {entry} for {key}", entry.GetFullId(), key);
                    var scheduled = _executor.Schedule(async () =>
                    {
                        try
                        {
                            this.Trace("await init of entry {entry} for {key}", entry.GetFullId(), key);
                            await entry.WorkerBase.InitAsync(key);

                            this.Trace("mark started entry {entry} for {key}", entry.GetFullId(), key);
                            entry.SetStarted();
                        }
                        catch (Exception e)
                        {
                            // the caller is waiting on WhenStarted: the executor would otherwise log this
                            // and move on, leaving StartAsync awaiting a signal nobody will ever set. The
                            // entry is dropped too, so a later StartAsync builds a fresh worker instead of
                            // awaiting the failure of this one forever
                            this.Error(e);
                            lock (_entries)
                                _entries.Remove(key);
                            entry.SetStartFailed(e);
                        }
                    });

                    // an executor that refused the work will never signal the entry, and the caller is
                    // about to await exactly that signal
                    if (!scheduled)
                    {
                        this.Trace("executor refused init of entry {entry} for {key}", entry.GetFullId(), key);
                        _entries.Remove(key);
                        entry.SetStartFailed(new ObjectDisposedException(nameof(WorkerManager<>)));
                    }
                }
            }

            if (pendingStop is not null)
            {
                try
                {
#pragma warning disable VSTHRD003
                    await pendingStop;
#pragma warning restore VSTHRD003
                }
                catch (Exception)
                {
                    // a worker that failed to stop is gone from the manager either way, and whoever asked
                    // for that stop was told; this caller only needs the key free
                }

                continue;
            }

            this.Trace("await start of entry {entry} for {key}", entry.GetFullId(), key);
            await entry.WhenStarted;

            this.Trace("done for {key}", key);

            return;
        }
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
                    try
                    {
                        // the executor is concurrent, so this task can begin while the entry's own init
                        // task is still running. Disposing then runs the worker's StopAsync alongside its
                        // StartAsync, on the same instance, tearing down what the start has not finished
                        // setting up
                        this.Trace("await start of entry {entry} for {key}", entry.GetFullId(), key);
                        try
                        {
#pragma warning disable VSTHRD003
                            await entry.WhenStarted;
#pragma warning restore VSTHRD003
                        }
                        catch (Exception)
                        {
                            // a worker that failed to start has already reported that to whoever asked for
                            // the start; disposing it anyway releases whatever it opened before failing
                        }

                        this.Trace("await disposal of entry {entry} for {key}", entry.GetFullId(), key);
                        await entry.WorkerBase.DisposeAsync();

                        this.Trace("remove entry of entry {entry} for {key}", entry.GetFullId(), key);
                        lock (_entries)
                            _entries.Remove(key);

                        this.Trace("mark stopped entry of entry {entry} for {key}", entry.GetFullId(), key);
                        entry.SetStopped();
                    }
                    catch (Exception e)
                    {
                        // same reasoning as the start path: a worker that fails to stop is still gone from
                        // the manager, and the caller learns why instead of waiting forever
                        this.Error(e);
                        lock (_entries)
                            _entries.Remove(key);
                        entry.SetStopFailed(e);
                    }
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

        this.Trace("take entries");
        Entry[] entries;
        lock (_entries)
        {
            entries = _entries.Values.ToArray();
            _entries.Clear();
        }

        // a worker started and never stopped still holds whatever it opened, and the manager is the only
        // thing left that knows about it. Failures are logged per worker so one bad stop does not strand the rest
        this.Trace<int>("dispose {count} remaining workers", entries.Length);
        foreach (var entry in entries)
        {
            try
            {
                this.Trace<string>("await disposal of entry {entry}", entry.GetFullId());
                await entry.WorkerBase.DisposeAsync();
                entry.SetStopped();
            }
            catch (Exception e)
            {
                this.Error(e);
                entry.SetStopFailed(e);
            }
        }

        this.Trace("done");
    }

    /// <summary>
    /// Ensures that the worker manager has not been disposed
    /// </summary>
    private void EnsureIsNotDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(WorkerManager<>));
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
        public void SetStarted() => _startedTcs.TrySetResult();

        /// <summary>
        /// Fails the start task, so a caller awaiting the worker's start observes the failure
        /// </summary>
        /// <param name="error">The failure raised while starting the worker</param>
        public void SetStartFailed(Exception error) => _startedTcs.TrySetException(error);

        /// <summary>
        /// Marks the worker as currently stopping
        /// </summary>
        public void SetIsStopping() => IsStopping = true;

        /// <summary>
        /// Marks the worker as stopped by completing the stop task
        /// </summary>
        public void SetStopped() => _stoppedTcs.TrySetResult();

        /// <summary>
        /// Fails the stop task, so a caller awaiting the worker's stop observes the failure
        /// </summary>
        /// <param name="error">The failure raised while stopping the worker</param>
        public void SetStopFailed(Exception error) => _stoppedTcs.TrySetException(error);
    }
}
