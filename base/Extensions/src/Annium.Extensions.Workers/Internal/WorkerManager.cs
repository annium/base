using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Execution;
using Annium.Logging;

namespace Annium.Extensions.Workers.Internal;

internal sealed class WorkerManager<TData> : IWorkerManager<TData>, IAsyncDisposable, ILogSubject
    where TData : IEquatable<TData>
{
    public ILogger Logger { get; }
    private readonly Dictionary<TData, Entry> _entries = new();
    private readonly IServiceProvider _sp;
    private readonly IBackgroundExecutor _executor;
    private bool _isDisposed;

    public WorkerManager(
        IServiceProvider sp,
        ILogger logger
    )
    {
        Logger = logger;
        _sp = sp;
        _executor = Executor.Background.Concurrent<WorkerManager<TData>>(logger);
        _executor.Start();
    }

    public void Start(TData key)
    {
        this.Trace("start");

        EnsureIsNotDisposed();

        Entry entry;
        lock (_entries)
        {
            if (_entries.ContainsKey(key))
            {
                this.Trace("skip, already started");

                return;
            }

            _entries[key] = entry = new Entry(_sp.Resolve<IWorker<TData>>());
        }

        this.Trace("schedule worker run");
        _executor.Schedule(() => entry.Worker.RunAsync(key, entry.Cts.Token));

        this.Trace("done");
    }

    public void Stop(TData key)
    {
        this.Trace("start");

        EnsureIsNotDisposed();

        lock (_entries)
        {
            if (_entries.Remove(key, out var entry))
            {
                this.Trace("cancel worker run");
                entry.Cts.Cancel();
            }
            else
                this.Trace("skip, entry not found");
        }

        this.Trace("done");
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
            throw new ObjectDisposedException(nameof(WorkerManager<TData>));
    }

    private record Entry(IWorker<TData> Worker)
    {
        public CancellationTokenSource Cts { get; } = new();
    }
}