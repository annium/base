using System;
using System.Collections.Generic;
using System.Threading;
using Annium.Logging;

namespace Annium;

/// <summary>
/// Provides a base class for disposable boxes that manage resources and provide thread-safe operations.
/// </summary>
/// <typeparam name="TBox">The type of the derived box class.</typeparam>
/// <remarks>
/// Established invariant for <see cref="DisposeBase"/> and derived dispose paths: <see cref="IsDisposed"/>
/// is set to <c>true</c> under <see cref="_locker"/>, then the actual list iteration runs OUTSIDE the
/// lock via <see cref="Pull{T}"/>, which atomically snapshots and clears the list under <see cref="_locker"/>.
/// Concurrent <see cref="Add{T}(List{T},T)"/> / <see cref="Remove{T}(List{T},T)"/> repeat the
/// <see cref="EnsureNotDisposed"/> check INSIDE the lock so a racing dispose cannot strand new entries.
/// Derived classes that own additional disposable lists MUST clear them in their own <see cref="Reset"/>
/// override AND drain them in their own dispose path under the same lock-then-pull pattern.
/// </remarks>
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
    /// Adds a single entry to the specified list. The disposed check is repeated INSIDE
    /// the lock so a concurrent dispose cannot strand the entry in the list after
    /// <see cref="DisposeBase"/> has already pulled the snapshot.
    /// </summary>
    /// <typeparam name="T">The type of the entry.</typeparam>
    /// <param name="entries">The list to add the entry to.</param>
    /// <param name="entry">The entry to add.</param>
    /// <returns>The current box instance for method chaining.</returns>
    protected TBox Add<T>(List<T> entries, T entry)
    {
        lock (_locker)
        {
            EnsureNotDisposed();
            this.Trace<string>("add {entry}", entry.GetFullId());
            entries.Add(entry);
        }

        return (TBox)this;
    }

    /// <summary>
    /// Adds a collection of entries to the specified list. The disposed check is repeated
    /// INSIDE the lock so a concurrent dispose cannot strand any entry.
    /// </summary>
    /// <typeparam name="T">The type of the entries.</typeparam>
    /// <param name="entries">The list to add the entries to.</param>
    /// <param name="items">The entries to add.</param>
    /// <returns>The current box instance for method chaining.</returns>
    protected TBox Add<T>(List<T> entries, IEnumerable<T> items)
    {
        lock (_locker)
        {
            EnsureNotDisposed();
            foreach (var entry in items)
            {
                this.Trace<string>("add {entry}", entry.GetFullId());
                entries.Add(entry);
            }
        }

        return (TBox)this;
    }

    /// <summary>
    /// Removes a single entry from the specified list. The disposed check runs INSIDE
    /// the lock to prevent a TOCTOU race against <see cref="DisposeBase"/>.
    /// </summary>
    /// <typeparam name="T">The type of the entry.</typeparam>
    /// <param name="entries">The list to remove the entry from.</param>
    /// <param name="item">The entry to remove.</param>
    /// <returns>The current box instance for method chaining.</returns>
    protected TBox Remove<T>(List<T> entries, T item)
    {
        lock (_locker)
        {
            EnsureNotDisposed();
            this.Trace<string>("remove {entry}", item.GetFullId());
            entries.Remove(item);
        }

        return (TBox)this;
    }

    /// <summary>
    /// Removes a collection of entries from the specified list. The disposed check runs
    /// INSIDE the lock to prevent a TOCTOU race against <see cref="DisposeBase"/>.
    /// </summary>
    /// <typeparam name="T">The type of the entries.</typeparam>
    /// <param name="entries">The list to remove the entries from.</param>
    /// <param name="items">The entries to remove.</param>
    /// <returns>The current box instance for method chaining.</returns>
    protected TBox Remove<T>(List<T> entries, IEnumerable<T> items)
    {
        lock (_locker)
        {
            EnsureNotDisposed();
            foreach (var item in items)
            {
                this.Trace<string>("remove {entry}", item.GetFullId());
                entries.Remove(item);
            }
        }

        return (TBox)this;
    }

    /// <summary>
    /// Atomically snapshots and clears the specified list under <c>_locker</c>.
    /// </summary>
    /// <typeparam name="T">The type of the entries.</typeparam>
    /// <param name="entries">The list to pull entries from.</param>
    /// <returns>A read-only collection containing all entries that were in the list at the time of the call.</returns>
    protected IReadOnlyCollection<T> Pull<T>(List<T> entries)
    {
        lock (_locker)
        {
            var slice = entries.ToArray();
            entries.Clear();
            return slice;
        }
    }

    /// <summary>
    /// Disposes all resources in the base box. Sets <see cref="IsDisposed"/> under the lock, then drains the
    /// sync lists outside the lock via <see cref="Pull{T}"/>. Derived classes that own additional disposable
    /// lists must drain those lists themselves following the same lock-then-pull invariant.
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
    /// Resets the box to its initial state under <see cref="_locker"/>. Derived classes that own additional
    /// disposable lists MUST override <see cref="ResetCore"/> to clear those lists under the same lock;
    /// otherwise stale entries would survive the next add+dispose cycle.
    /// </summary>
    protected void Reset()
    {
        lock (_locker)
        {
            IsDisposed = false;
            SyncDisposables.Clear();
            SyncDisposes.Clear();
            ResetCore();
        }
    }

    /// <summary>
    /// Hook invoked under <see cref="_locker"/> from <see cref="Reset"/> for derived classes to clear any
    /// additional disposable lists they own. Default implementation is a no-op.
    /// </summary>
    protected virtual void ResetCore() { }

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
