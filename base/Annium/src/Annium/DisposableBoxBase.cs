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
/// Concurrent <see cref="AddSyncDisposable"/> / <see cref="RemoveSyncDisposable"/> (and their list
/// equivalents) repeat the disposed check INSIDE the lock so a racing dispose cannot strand new entries.
/// Derived classes that own additional disposable lists pass them to the protected generic
/// <c>Add</c>/<c>Remove</c>/<c>Pull</c> helpers (their own private fields stay encapsulated) and override
/// <see cref="ResetCore"/> to clear them under the same lock.
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
    /// Private list of synchronous disposable resources owned by the base. Mutated only via the protected
    /// <see cref="AddSyncDisposable"/> / <see cref="AddSyncDisposables"/> /
    /// <see cref="RemoveSyncDisposable"/> / <see cref="RemoveSyncDisposables"/> helpers, which centralize
    /// the lock semantics. Kept private so derived classes cannot bypass the lock by mutating the list directly.
    /// </summary>
    private readonly List<IDisposable> _syncDisposables = new();

    /// <summary>
    /// Private list of synchronous dispose actions, mutated only via the protected helpers. See
    /// <see cref="_syncDisposables"/> for the rationale.
    /// </summary>
    private readonly List<Action> _syncDisposes = new();

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
    /// <param name="entries">The list to add the entry to (typically a derived class's own private list).</param>
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

    /// <summary>Adds a synchronous <see cref="IDisposable"/> to the base's sync-disposables list.</summary>
    /// <param name="disposable">The disposable to add.</param>
    /// <returns>This box instance.</returns>
    protected TBox AddSyncDisposable(IDisposable disposable) => Add(_syncDisposables, disposable);

    /// <summary>Adds a collection of synchronous <see cref="IDisposable"/>s to the base's sync-disposables list.</summary>
    /// <param name="disposables">The disposables to add.</param>
    /// <returns>This box instance.</returns>
    protected TBox AddSyncDisposables(IEnumerable<IDisposable> disposables) => Add(_syncDisposables, disposables);

    /// <summary>Removes a synchronous <see cref="IDisposable"/> from the base's sync-disposables list.</summary>
    /// <param name="disposable">The disposable to remove.</param>
    /// <returns>This box instance.</returns>
    protected TBox RemoveSyncDisposable(IDisposable disposable) => Remove(_syncDisposables, disposable);

    /// <summary>Removes a collection of synchronous <see cref="IDisposable"/>s from the base's sync-disposables list.</summary>
    /// <param name="disposables">The disposables to remove.</param>
    /// <returns>This box instance.</returns>
    protected TBox RemoveSyncDisposables(IEnumerable<IDisposable> disposables) => Remove(_syncDisposables, disposables);

    /// <summary>Adds a synchronous dispose <see cref="Action"/> to the base's sync-disposes list.</summary>
    /// <param name="dispose">The dispose action to add.</param>
    /// <returns>This box instance.</returns>
    protected TBox AddSyncDispose(Action dispose) => Add(_syncDisposes, dispose);

    /// <summary>Adds a collection of synchronous dispose <see cref="Action"/>s to the base's sync-disposes list.</summary>
    /// <param name="disposes">The dispose actions to add.</param>
    /// <returns>This box instance.</returns>
    protected TBox AddSyncDisposes(IEnumerable<Action> disposes) => Add(_syncDisposes, disposes);

    /// <summary>Removes a synchronous dispose <see cref="Action"/> from the base's sync-disposes list.</summary>
    /// <param name="dispose">The dispose action to remove.</param>
    /// <returns>This box instance.</returns>
    protected TBox RemoveSyncDispose(Action dispose) => Remove(_syncDisposes, dispose);

    /// <summary>Removes a collection of synchronous dispose <see cref="Action"/>s from the base's sync-disposes list.</summary>
    /// <param name="disposes">The dispose actions to remove.</param>
    /// <returns>This box instance.</returns>
    protected TBox RemoveSyncDisposes(IEnumerable<Action> disposes) => Remove(_syncDisposes, disposes);

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

        foreach (var entry in Pull(_syncDisposables))
        {
            this.Trace<string>("dispose {entry} - start", entry.GetFullId());
            entry.Dispose();
            this.Trace<string>("dispose {entry} - done", entry.GetFullId());
        }

        foreach (var entry in Pull(_syncDisposes))
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
            _syncDisposables.Clear();
            _syncDisposes.Clear();
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
