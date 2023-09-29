using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium;

public sealed class AsyncDisposableBox : DisposableBoxBase<AsyncDisposableBox>, IAsyncDisposable
{
    private readonly List<IAsyncDisposable> _asyncDisposables = new();
    private readonly List<Func<Task>> _asyncDisposes = new();

    internal AsyncDisposableBox(ILogger logger)
        : base(logger)
    {
    }

    public async ValueTask DisposeAndResetAsync()
    {
        await DisposeAsync();
        Reset();
    }

    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        DisposeBase();

        if (_asyncDisposables.Count > 0)
            await Task.WhenAll(Pull(_asyncDisposables).Select(async entry =>
            {
                this.Trace<string>("dispose {entry} - start", entry.GetFullId());
                await entry.DisposeAsync();
                this.Trace<string>("dispose {entry} - done", entry.GetFullId());
            }));
        if (_asyncDisposes.Count > 0)
            await Task.WhenAll(Pull(_asyncDisposes).Select(async entry =>
            {
                this.Trace<string>("dispose {entry} - start", entry.GetFullId());
                await entry();
                this.Trace<string>("dispose {entry} - done", entry.GetFullId());
            }));

        this.Trace("done");
    }

    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IDisposable disposable) => box.Add(box.SyncDisposables, disposable);
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IDisposable disposable) => box.Remove(box.SyncDisposables, disposable);
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<IDisposable> disposables) => box.Add(box.SyncDisposables, disposables);
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<IDisposable> disposables) => box.Remove(box.SyncDisposables, disposables);
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IAsyncDisposable disposable) => box.Add(box._asyncDisposables, disposable);
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IAsyncDisposable disposable) => box.Remove(box._asyncDisposables, disposable);
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<IAsyncDisposable> disposables) => box.Add(box._asyncDisposables, disposables);
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<IAsyncDisposable> disposables) => box.Remove(box._asyncDisposables, disposables);
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, Action dispose) => box.Add(box.SyncDisposes, dispose);
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, Action dispose) => box.Remove(box.SyncDisposes, dispose);
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<Action> disposes) => box.Add(box.SyncDisposes, disposes);
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<Action> disposes) => box.Remove(box.SyncDisposes, disposes);
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, Func<Task> dispose) => box.Add(box._asyncDisposes, dispose);
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, Func<Task> dispose) => box.Remove(box._asyncDisposes, dispose);
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<Func<Task>> disposes) => box.Add(box._asyncDisposes, disposes);
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<Func<Task>> disposes) => box.Remove(box._asyncDisposes, disposes);
}

public sealed class DisposableBox : DisposableBoxBase<DisposableBox>, IDisposable
{
    internal DisposableBox(ILogger logger)
        : base(logger)
    {
    }

    public void DisposeAndReset()
    {
        Dispose();
        Reset();
    }

    public void Dispose()
    {
        this.Trace("start");
        DisposeBase();
        this.Trace("done");
    }

    public static DisposableBox operator +(DisposableBox box, IDisposable disposable) => box.Add(box.SyncDisposables, disposable);
    public static DisposableBox operator -(DisposableBox box, IDisposable disposable) => box.Remove(box.SyncDisposables, disposable);
    public static DisposableBox operator +(DisposableBox box, IEnumerable<IDisposable> disposables) => box.Add(box.SyncDisposables, disposables);
    public static DisposableBox operator -(DisposableBox box, IEnumerable<IDisposable> disposables) => box.Remove(box.SyncDisposables, disposables);
    public static DisposableBox operator +(DisposableBox box, Action dispose) => box.Add(box.SyncDisposes, dispose);
    public static DisposableBox operator -(DisposableBox box, Action dispose) => box.Remove(box.SyncDisposes, dispose);
    public static DisposableBox operator +(DisposableBox box, IEnumerable<Action> disposes) => box.Add(box.SyncDisposes, disposes);
    public static DisposableBox operator -(DisposableBox box, IEnumerable<Action> disposes) => box.Remove(box.SyncDisposes, disposes);
}

public abstract class DisposableBoxBase<TBox> : ILogSubject
    where TBox : DisposableBoxBase<TBox>
{
    public ILogger Logger { get; }
    public bool IsDisposed { get; private set; }
    protected readonly List<IDisposable> SyncDisposables = new();
    protected readonly List<Action> SyncDisposes = new();
    private readonly object _locker = new();

    protected DisposableBoxBase(ILogger logger)
    {
        Logger = logger;
    }

    protected TBox Add<T>(List<T> entries, T entry)
    {
        EnsureNotDisposed();

        lock (_locker)
        {
            this.Trace<string>("add {entry}", entry.GetFullId());
            entries.Add(entry);
        }

        return (TBox)this;
    }

    protected TBox Add<T>(List<T> entries, IEnumerable<T> items)
    {
        EnsureNotDisposed();

        lock (_locker)
            foreach (var entry in items)
            {
                this.Trace<string>("add {entry}", entry.GetFullId());
                entries.Add(entry);
            }

        return (TBox)this;
    }

    protected TBox Remove<T>(List<T> entries, T item)
    {
        EnsureNotDisposed();

        lock (_locker)
        {
            this.Trace<string>("remove {entry}", item.GetFullId());
            entries.Remove(item);
        }

        return (TBox)this;
    }

    protected TBox Remove<T>(List<T> entries, IEnumerable<T> items)
    {
        EnsureNotDisposed();

        lock (_locker)
            foreach (var item in items)
            {
                this.Trace<string>("remove {entry}", item.GetFullId());
                entries.Remove(item);
            }

        return (TBox)this;
    }

    protected IReadOnlyCollection<T> Pull<T>(List<T> entries)
    {
        var slice = entries.ToArray();
        entries.Clear();

        return slice;
    }

    protected void DisposeBase()
    {
        lock (_locker)
        {
            if (IsDisposed)
            {
                this.Trace("already disposed");
                return;
            }

            IsDisposed = true;
        }

        if (SyncDisposables.Count > 0)
            foreach (var entry in Pull(SyncDisposables))
            {
                this.Trace<string>("dispose {entry} - start", entry.GetFullId());
                entry.Dispose();
                this.Trace<string>("dispose {entry} - done", entry.GetFullId());
            }

        if (SyncDisposes.Count > 0)
            foreach (var entry in Pull(SyncDisposes))
            {
                this.Trace<string>("dispose {entry} - start", entry.GetFullId());
                entry();
                this.Trace<string>("dispose {entry} - done", entry.GetFullId());
            }
    }

    protected void Reset()
    {
        lock (_locker)
        {
            if (!IsDisposed)
                throw new InvalidOperationException($"Can't reset {typeof(TBox).FriendlyName()} - it's not disposed");

            IsDisposed = false;
        }
    }

    private void EnsureNotDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(typeof(TBox).Name);
    }
}