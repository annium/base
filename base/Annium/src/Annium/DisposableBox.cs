using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium;

/// <summary>
/// Represents a box that manages asynchronous disposable resources and provides thread-safe operations for adding and removing them.
/// </summary>
public sealed class AsyncDisposableBox : DisposableBoxBase<AsyncDisposableBox>, IAsyncDisposable
{
    /// <summary>
    /// A list of asynchronous disposable resources managed by this box.
    /// </summary>
    private readonly List<IAsyncDisposable> _asyncDisposables = new();

    /// <summary>
    /// A list of asynchronous dispose functions managed by this box.
    /// </summary>
    private readonly List<Func<Task>> _asyncDisposes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncDisposableBox"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    internal AsyncDisposableBox(ILogger logger)
        : base(logger) { }

    /// <summary>
    /// Disposes all resources and resets the box to its initial state.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask DisposeAndResetAsync()
    {
        await DisposeAsync();
        Reset();
    }

    /// <summary>
    /// Asynchronously disposes all resources in the box.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        DisposeBase();

        if (_asyncDisposables.Count > 0)
            await Task.WhenAll(
                Pull(_asyncDisposables)
                    .Select(async entry =>
                    {
                        this.Trace<string>("dispose {entry} - start", entry.GetFullId());
                        await entry.DisposeAsync();
                        this.Trace<string>("dispose {entry} - done", entry.GetFullId());
                    })
            );
        if (_asyncDisposes.Count > 0)
            await Task.WhenAll(
                Pull(_asyncDisposes)
                    .Select(async entry =>
                    {
                        this.Trace<string>("dispose {entry} - start", entry.GetFullId());
                        await entry();
                        this.Trace<string>("dispose {entry} - done", entry.GetFullId());
                    })
            );

        this.Trace("done");
    }

    /// <summary>
    /// Adds a synchronous disposable resource to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IDisposable disposable) =>
        box.Add(box.SyncDisposables, disposable);

    /// <summary>
    /// Removes a synchronous disposable resource from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IDisposable disposable) =>
        box.Remove(box.SyncDisposables, disposable);

    /// <summary>
    /// Adds a collection of synchronous disposable resources to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<IDisposable> disposables) =>
        box.Add(box.SyncDisposables, disposables);

    /// <summary>
    /// Removes a collection of synchronous disposable resources from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<IDisposable> disposables) =>
        box.Remove(box.SyncDisposables, disposables);

    /// <summary>
    /// Adds an asynchronous disposable resource to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IAsyncDisposable disposable) =>
        box.Add(box._asyncDisposables, disposable);

    /// <summary>
    /// Removes an asynchronous disposable resource from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IAsyncDisposable disposable) =>
        box.Remove(box._asyncDisposables, disposable);

    /// <summary>
    /// Adds a collection of asynchronous disposable resources to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<IAsyncDisposable> disposables) =>
        box.Add(box._asyncDisposables, disposables);

    /// <summary>
    /// Removes a collection of asynchronous disposable resources from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<IAsyncDisposable> disposables) =>
        box.Remove(box._asyncDisposables, disposables);

    /// <summary>
    /// Adds a synchronous dispose action to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, Action dispose) =>
        box.Add(box.SyncDisposes, dispose);

    /// <summary>
    /// Removes a synchronous dispose action from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, Action dispose) =>
        box.Remove(box.SyncDisposes, dispose);

    /// <summary>
    /// Adds a collection of synchronous dispose actions to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<Action> disposes) =>
        box.Add(box.SyncDisposes, disposes);

    /// <summary>
    /// Removes a collection of synchronous dispose actions from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<Action> disposes) =>
        box.Remove(box.SyncDisposes, disposes);

    /// <summary>
    /// Adds an asynchronous dispose function to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, Func<Task> dispose) =>
        box.Add(box._asyncDisposes, dispose);

    /// <summary>
    /// Removes an asynchronous dispose function from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, Func<Task> dispose) =>
        box.Remove(box._asyncDisposes, dispose);

    /// <summary>
    /// Adds a collection of asynchronous dispose functions to the box.
    /// </summary>
    public static AsyncDisposableBox operator +(AsyncDisposableBox box, IEnumerable<Func<Task>> disposes) =>
        box.Add(box._asyncDisposes, disposes);

    /// <summary>
    /// Removes a collection of asynchronous dispose functions from the box.
    /// </summary>
    public static AsyncDisposableBox operator -(AsyncDisposableBox box, IEnumerable<Func<Task>> disposes) =>
        box.Remove(box._asyncDisposes, disposes);
}

/// <summary>
/// Represents a box that manages synchronous disposable resources and provides thread-safe operations for adding and removing them.
/// </summary>
public sealed class DisposableBox : DisposableBoxBase<DisposableBox>, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableBox"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    internal DisposableBox(ILogger logger)
        : base(logger) { }

    /// <summary>
    /// Disposes all resources and resets the box to its initial state.
    /// </summary>
    public void DisposeAndReset()
    {
        Dispose();
        Reset();
    }

    /// <summary>
    /// Disposes all resources in the box.
    /// </summary>
    public void Dispose()
    {
        this.Trace("start");
        DisposeBase();
        this.Trace("done");
    }

    /// <summary>
    /// Adds a disposable resource to the box.
    /// </summary>
    public static DisposableBox operator +(DisposableBox box, IDisposable disposable) =>
        box.Add(box.SyncDisposables, disposable);

    /// <summary>
    /// Removes a disposable resource from the box.
    /// </summary>
    public static DisposableBox operator -(DisposableBox box, IDisposable disposable) =>
        box.Remove(box.SyncDisposables, disposable);

    /// <summary>
    /// Adds a collection of disposable resources to the box.
    /// </summary>
    public static DisposableBox operator +(DisposableBox box, IEnumerable<IDisposable> disposables) =>
        box.Add(box.SyncDisposables, disposables);

    /// <summary>
    /// Removes a collection of disposable resources from the box.
    /// </summary>
    public static DisposableBox operator -(DisposableBox box, IEnumerable<IDisposable> disposables) =>
        box.Remove(box.SyncDisposables, disposables);

    /// <summary>
    /// Adds a dispose action to the box.
    /// </summary>
    public static DisposableBox operator +(DisposableBox box, Action dispose) => box.Add(box.SyncDisposes, dispose);

    /// <summary>
    /// Removes a dispose action from the box.
    /// </summary>
    public static DisposableBox operator -(DisposableBox box, Action dispose) => box.Remove(box.SyncDisposes, dispose);

    /// <summary>
    /// Adds a collection of dispose actions to the box.
    /// </summary>
    public static DisposableBox operator +(DisposableBox box, IEnumerable<Action> disposes) =>
        box.Add(box.SyncDisposes, disposes);

    /// <summary>
    /// Removes a collection of dispose actions from the box.
    /// </summary>
    public static DisposableBox operator -(DisposableBox box, IEnumerable<Action> disposes) =>
        box.Remove(box.SyncDisposes, disposes);
}

/// <summary>
/// Provides a base class for disposable boxes that manage resources and provide thread-safe operations.
/// </summary>
/// <typeparam name="TBox">The type of the derived box class.</typeparam>
public abstract class DisposableBoxBase<TBox> : ILogSubject
    where TBox : DisposableBoxBase<TBox>
{
    /// <summary>
    /// Gets the logger instance for tracing operations.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets a value indicating whether the box has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// The list of synchronous disposable resources.
    /// </summary>
    protected readonly List<IDisposable> SyncDisposables = new();

    /// <summary>
    /// The list of synchronous dispose actions.
    /// </summary>
    protected readonly List<Action> SyncDisposes = new();

    /// <summary>
    /// A thread-safe lock object used to synchronize access to the box's resources.
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposableBoxBase{TBox}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    protected DisposableBoxBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Adds a single entry to the specified list.
    /// </summary>
    /// <typeparam name="T">The type of the entry.</typeparam>
    /// <param name="entries">The list to add the entry to.</param>
    /// <param name="entry">The entry to add.</param>
    /// <returns>The current box instance for method chaining.</returns>
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

    /// <summary>
    /// Adds a collection of entries to the specified list.
    /// </summary>
    /// <typeparam name="T">The type of the entries.</typeparam>
    /// <param name="entries">The list to add the entries to.</param>
    /// <param name="items">The entries to add.</param>
    /// <returns>The current box instance for method chaining.</returns>
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

    /// <summary>
    /// Removes a single entry from the specified list.
    /// </summary>
    /// <typeparam name="T">The type of the entry.</typeparam>
    /// <param name="entries">The list to remove the entry from.</param>
    /// <param name="item">The entry to remove.</param>
    /// <returns>The current box instance for method chaining.</returns>
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

    /// <summary>
    /// Removes a collection of entries from the specified list.
    /// </summary>
    /// <typeparam name="T">The type of the entries.</typeparam>
    /// <param name="entries">The list to remove the entries from.</param>
    /// <param name="items">The entries to remove.</param>
    /// <returns>The current box instance for method chaining.</returns>
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

    /// <summary>
    /// Pulls all entries from the specified list and returns them as a read-only collection.
    /// </summary>
    /// <typeparam name="T">The type of the entries.</typeparam>
    /// <param name="entries">The list to pull entries from.</param>
    /// <returns>A read-only collection containing all entries from the list.</returns>
    protected IReadOnlyCollection<T> Pull<T>(List<T> entries)
    {
        var slice = entries.ToArray();
        entries.Clear();

        return slice;
    }

    /// <summary>
    /// Disposes all resources in the base box.
    /// </summary>
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

    /// <summary>
    /// Resets the box to its initial state.
    /// </summary>
    protected void Reset()
    {
        lock (_locker)
        {
            IsDisposed = false;
            SyncDisposables.Clear();
            SyncDisposes.Clear();
        }
    }

    /// <summary>
    /// Ensures that the box has not been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the box has already been disposed.</exception>
    private void EnsureNotDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(GetType().Name);
    }
}
