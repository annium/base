using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Data.Tables.Internal;

/// <summary>
/// Internal implementation of a reactive table that manages a collection of items with change notifications.
/// </summary>
/// <typeparam name="T">The type of items stored in the table.</typeparam>
internal sealed class Table<T> : ITable<T>, ILogSubject
    where T : notnull
{
    /// <summary>
    /// Gets the logger instance for this table.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the current number of items in the table.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_locker)
                return _table.Count;
        }
    }

    /// <summary>
    /// Gets a snapshot of the current table data as a read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<int, T> Source
    {
        get
        {
            lock (_locker)
                return _table.ToDictionary();
        }
    }

    /// <summary>
    /// Internal dictionary storing the table data.
    /// </summary>
    private readonly Dictionary<int, T> _table = new();

    /// <summary>
    /// Function to extract keys from items.
    /// </summary>
    private readonly GetKey<T> _getKey;

    /// <summary>
    /// Function to determine if an item has changed.
    /// </summary>
    private readonly HasChanged<T, T> _hasChanged;

    /// <summary>
    /// Function to update an existing item with new data.
    /// </summary>
    private readonly Update<T, T> _update;

    /// <summary>
    /// Function to determine if an item should remain active in the table.
    /// </summary>
    private readonly Func<T, bool> _isActive;

    /// <summary>
    /// Lock for thread-safe access to the table.
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// Cancellation token source for the observable stream.
    /// </summary>
    private readonly CancellationTokenSource _observableCts = new();

    /// <summary>
    /// Guards against repeated disposal so DisposeAsync is idempotent (0 = live, 1 = disposed).
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Observable stream of change events.
    /// </summary>
    private readonly IObservable<ChangeEvent<T>> _observable;

    /// <summary>
    /// Permissions defining what operations are allowed on this table.
    /// </summary>
    private readonly TablePermission _permissions;

    /// <summary>
    /// Channel writer for publishing change events.
    /// </summary>
    private readonly ChannelWriter<ChangeEvent<T>> _eventWriter;

    /// <summary>
    /// Channel reader for consuming change events.
    /// </summary>
    private readonly ChannelReader<ChangeEvent<T>> _eventReader;

    /// <summary>
    /// Initializes a new instance of the Table class.
    /// </summary>
    /// <param name="permissions">The permissions defining allowed operations.</param>
    /// <param name="getKey">Function to extract keys from items.</param>
    /// <param name="hasChanged">Function to determine if an item has changed.</param>
    /// <param name="update">Function to update an existing item with new data.</param>
    /// <param name="isActive">Function to determine if an item should remain active.</param>
    /// <param name="logger">Logger instance for this table.</param>
    public Table(
        TablePermission permissions,
        GetKey<T> getKey,
        HasChanged<T, T> hasChanged,
        Update<T, T> update,
        Func<T, bool> isActive,
        ILogger logger
    )
    {
        Logger = logger;
        _getKey = getKey;
        _hasChanged = hasChanged;
        _update = update;
        _isActive = isActive;
        _permissions = permissions;

        var taskChannel = Channel.CreateUnbounded<ChangeEvent<T>>(
            new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleWriter = false,
                SingleReader = true,
            }
        );
        _eventWriter = taskChannel.Writer;
        _eventReader = taskChannel.Reader;

        _observable = CreateObservable(_observableCts.Token, logger).TrackCompletion(logger);
    }

    /// <summary>
    /// Asynchronously disposes of the table resources.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        // IAsyncDisposable contract: DisposeAsync must be safely callable more than once,
        // including concurrently — only the first caller runs the teardown
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
            return;

        this.Trace("start, complete writer");
        _eventWriter.Complete();

        this.Trace("cancel observable");
        await _observableCts.CancelAsync();

        this.Trace("await observable completion");
        await _observable.WhenCompletedAsync(Logger);

        this.Trace("dispose cts");
        _observableCts.Dispose();

        this.Trace("clear table");
        lock (_locker)
            _table.Clear();

        this.Trace("done");
    }

    /// <summary>
    /// Subscribes an observer to receive change events from this table. The observer is
    /// subscribed to the live stream BEFORE the initial-state snapshot is taken, so no
    /// intervening mutations are lost: any <c>Set</c>/<c>Delete</c> that publishes an event
    /// after the subscription is active — but before this method returns — will be delivered.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>A disposable subscription.</returns>
    public IDisposable Subscribe(IObserver<ChangeEvent<T>> observer)
    {
        IDisposable subscription;
        ChangeEvent<T> initial;
        lock (_locker)
        {
            subscription = _observable.Subscribe(observer);
            initial = ChangeEvent.Init(_table.Values.ToArray());
        }

        // dispatch initial event OUTSIDE the lock so observer code never runs under _locker
        observer.OnNext(initial);
        return subscription;
    }

    /// <summary>
    /// Gets the key for the specified item.
    /// </summary>
    /// <param name="value">The item to get the key for.</param>
    /// <returns>The key for the item.</returns>
    public int GetKey(T value) => _getKey(value);

    /// <summary>
    /// Initializes the table with the specified collection of entries. The <c>_isActive</c>
    /// filter is applied OUTSIDE the lock so that user delegates never run under <c>_locker</c>
    /// (which is a non-reentrant <see cref="Lock"/>).
    /// </summary>
    /// <param name="entries">The entries to initialize the table with.</param>
    /// <exception cref="InvalidOperationException">Thrown when the table does not have Init permission.</exception>
    public void Init(IReadOnlyCollection<T> entries)
    {
        EnsurePermission(TablePermission.Init);

        // filter and key-extract outside the lock — user delegates must never run under _locker
        var activeByKey = entries.Where(x => _isActive(x)).Select(x => (Key: _getKey(x), Value: x)).ToArray();

        T[] snapshot;
        lock (_locker)
        {
            _table.Clear();
            foreach (var (key, value) in activeByKey)
                _table[key] = value;
            snapshot = _table.Values.ToArray();
            AddEvent(ChangeEvent.Init(snapshot));
        }
    }

    /// <summary>
    /// Sets (adds or updates) an entry in the table. User delegates (<c>_hasChanged</c>,
    /// <c>_update</c>, <c>_isActive</c>) are invoked OUTSIDE <c>_locker</c> so a delegate that
    /// re-enters the table (e.g. calls <c>Set</c> recursively) does not deadlock.
    /// </summary>
    /// <param name="entry">The entry to set.</param>
    /// <exception cref="InvalidOperationException">Thrown when the table does not have the required permissions.</exception>
    public void Set(T entry)
    {
        var key = _getKey(entry);

        T? existing;
        bool exists;
        lock (_locker)
        {
            exists = _table.TryGetValue(key, out existing);
        }

        if (exists)
        {
            EnsurePermission(TablePermission.Update);
            if (!_hasChanged(existing!, entry))
            {
                CleanupOutsideLock();
                return;
            }
            _update(existing!, entry);
            var isActive = _isActive(existing!);
            if (isActive)
            {
                lock (_locker)
                    // guard against a concurrent Delete that removed the key between the
                    // initial read and here — without this a Set event could be emitted
                    // after the Delete event for an item no longer in the table
                    if (_table.ContainsKey(key))
                        AddEvent(ChangeEvent.Set(existing!));
            }
        }
        else
        {
            EnsurePermission(TablePermission.Add);
            lock (_locker)
            {
                // another thread may have added the same new key after our initial read; only
                // add and emit the Set event once to avoid a duplicate Set for a single add
                if (!_table.ContainsKey(key))
                {
                    _table[key] = entry;
                    AddEvent(ChangeEvent.Set(entry));
                }
            }
        }

        CleanupOutsideLock();
    }

    /// <summary>
    /// Deletes an entry from the table.
    /// </summary>
    /// <param name="entry">The entry to delete.</param>
    /// <exception cref="InvalidOperationException">Thrown when the table does not have Delete permission.</exception>
    public void Delete(T entry)
    {
        EnsurePermission(TablePermission.Delete);
        var key = _getKey(entry);

        lock (_locker)
        {
            if (_table.Remove(key, out var item))
                AddEvent(ChangeEvent.Delete(item));
        }

        CleanupOutsideLock();
    }

    /// <summary>
    /// Adds multiple change events to the event stream.
    /// </summary>
    /// <param name="events">The events to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when an event cannot be sent.</exception>
    private void AddEvents(IReadOnlyCollection<ChangeEvent<T>> events)
    {
        foreach (var @event in events)
            if (!_eventWriter.TryWrite(@event))
                throw new InvalidOperationException("Event must have been sent.");
    }

    /// <summary>
    /// Adds a single change event to the event stream.
    /// </summary>
    /// <param name="event">The event to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the event cannot be sent.</exception>
    private void AddEvent(ChangeEvent<T> @event)
    {
        if (!_eventWriter.TryWrite(@event))
            throw new InvalidOperationException("Event must have been sent.");
    }

    /// <summary>
    /// Ensures that the table has the specified permission.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <exception cref="InvalidOperationException">Thrown when the table does not have the required permission.</exception>
    private void EnsurePermission(TablePermission permission)
    {
        if (!_permissions.HasFlag(permission))
            throw new InvalidOperationException($"Table {GetType().FriendlyName()} has no {permission} permission.");
    }

    /// <summary>
    /// Gets a snapshot of all items currently in the table.
    /// </summary>
    /// <returns>A read-only collection of all items in the table.</returns>
    private IReadOnlyCollection<T> Get()
    {
        lock (_locker)
            return _table.Values.ToArray();
    }

    /// <summary>
    /// Removes inactive items from the table and sends delete events for them. The
    /// <c>_isActive</c> delegate is invoked OUTSIDE <c>_locker</c>; only the dictionary
    /// access and event publication run under the lock.
    /// </summary>
    private void CleanupOutsideLock()
    {
        // snapshot current values under lock; filter outside lock (user delegate)
        T[] snapshot;
        lock (_locker)
            snapshot = _table.Values.ToArray();

        var inactive = snapshot.Where(x => !_isActive(x)).Select(x => (Key: _getKey(x), Value: x)).ToArray();
        if (inactive.Length == 0)
            return;

        var removed = new List<T>(inactive.Length);
        lock (_locker)
        {
            foreach (var (key, _) in inactive)
            {
                if (_table.Remove(key, out var item))
                    removed.Add(item!);
            }
            AddEvents(removed.Select(ChangeEvent.Delete).ToArray());
        }
    }

    /// <summary>
    /// Creates the observable stream for change events.
    /// </summary>
    /// <param name="ct">Cancellation token for the observable.</param>
    /// <param name="logger">Logger instance.</param>
    /// <returns>An observable stream of change events.</returns>
    private IObservable<ChangeEvent<T>> CreateObservable(CancellationToken ct, ILogger logger) =>
        ObservableExt.StaticSyncInstance<ChangeEvent<T>>(
            async ctx =>
            {
                try
                {
                    while (await _eventReader.WaitToReadAsync(ctx.Ct))
                    {
                        var message = await _eventReader.ReadAsync(ctx.Ct);

                        ctx.OnNext(message);
                    }
                }
                // token was canceled
                catch (OperationCanceledException) { }
                catch (ChannelClosedException) { }
                catch (Exception e)
                {
                    ctx.OnError(e);
                }

                return () => Task.CompletedTask;
            },
            ct,
            logger
        );

    /// <summary>
    /// Returns an enumerator that iterates through the table items.
    /// </summary>
    /// <returns>An enumerator for the table items.</returns>
    public IEnumerator<T> GetEnumerator() => Get().GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the table items.
    /// </summary>
    /// <returns>An enumerator for the table items.</returns>
    IEnumerator IEnumerable.GetEnumerator() => Get().GetEnumerator();
}
