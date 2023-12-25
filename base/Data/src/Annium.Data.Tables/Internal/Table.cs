using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Data.Tables.Internal;

internal sealed class Table<T> : ITable<T>, ILogSubject
    where T : notnull
{
    public ILogger Logger { get; }
    public int Count
    {
        get
        {
            lock (_dataLocker)
                return _table.Count;
        }
    }

    public IReadOnlyDictionary<int, T> Source
    {
        get
        {
            lock (_dataLocker)
                return _table.ToDictionary();
        }
    }

    private readonly Dictionary<int, T> _table = new();
    private readonly GetKey<T> _getKey;
    private readonly HasChanged<T, T> _hasChanged;
    private readonly Update<T, T> _update;
    private readonly Func<T, bool> _isActive;
    private readonly object _dataLocker = new();
    private readonly CancellationTokenSource _observableCts = new();
    private readonly IObservable<ChangeEvent<T>> _observable;
    private readonly TablePermission _permissions;
    private readonly ChannelWriter<ChangeEvent<T>> _eventWriter;
    private readonly ChannelReader<ChangeEvent<T>> _eventReader;

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
                SingleReader = true
            }
        );
        _eventWriter = taskChannel.Writer;
        _eventReader = taskChannel.Reader;

        _observable = CreateObservable(_observableCts.Token, logger).TrackCompletion(logger);
    }

    public async ValueTask DisposeAsync()
    {
        this.Trace("start, complete writer");
        _eventWriter.Complete();

        this.Trace("cancel observable");
        await _observableCts.CancelAsync();

        this.Trace("await observable completion");
        await _observable.WhenCompleted(Logger);

        this.Trace("clear table");
        lock (_dataLocker)
            _table.Clear();

        this.Trace("done");
    }

    public IDisposable Subscribe(IObserver<ChangeEvent<T>> observer)
    {
        var init = ChangeEvent.Init(Get());
        observer.OnNext(init);

        return _observable.Subscribe(observer);
    }

    public int GetKey(T value) => _getKey(value);

    public void Init(IReadOnlyCollection<T> entries)
    {
        EnsurePermission(TablePermission.Init);

        lock (_dataLocker)
        {
            _table.Clear();

            foreach (var entry in entries.Where(x => _isActive(x)))
            {
                var key = _getKey(entry);
                _table[key] = entry;
            }

            AddEvent(ChangeEvent.Init(_table.Values.ToArray()));
        }
    }

    public void Set(T entry)
    {
        var key = _getKey(entry);

        lock (_dataLocker)
        {
            var exists = _table.ContainsKey(key);
            if (exists)
            {
                EnsurePermission(TablePermission.Update);
                var value = _table[key];
                var hasChanged = _hasChanged(value, entry);
                if (hasChanged)
                {
                    _update(value, entry);
                    if (_isActive(value))
                        AddEvent(ChangeEvent.Set(value));
                }
            }
            else
            {
                EnsurePermission(TablePermission.Add);
                var value = _table[key] = entry;
                AddEvent(ChangeEvent.Set(value));
            }

            Cleanup();
        }
    }

    public void Delete(T entry)
    {
        EnsurePermission(TablePermission.Delete);
        var key = _getKey(entry);

        lock (_dataLocker)
        {
            if (_table.Remove(key, out var item))
                AddEvent(ChangeEvent.Delete(item));

            Cleanup();
        }
    }

    private void AddEvents(IReadOnlyCollection<ChangeEvent<T>> events)
    {
        foreach (var @event in events)
            if (!_eventWriter.TryWrite(@event))
                throw new InvalidOperationException("Event must have been sent.");
    }

    private void AddEvent(ChangeEvent<T> @event)
    {
        if (!_eventWriter.TryWrite(@event))
            throw new InvalidOperationException("Event must have been sent.");
    }

    private void EnsurePermission(TablePermission permission)
    {
        if (!_permissions.HasFlag(permission))
            throw new InvalidOperationException($"Table {GetType().FriendlyName()} has no {permission} permission.");
    }

    private IReadOnlyCollection<T> Get()
    {
        lock (_dataLocker)
            return _table.Values.ToArray();
    }

    private void Cleanup()
    {
        var removed = new List<T>();

        var entries = _table.Values.Except(_table.Values.Where(_isActive)).ToArray();
        foreach (var entry in entries)
        {
            var key = _getKey(entry);
            _table.Remove(key, out var item);
            removed.Add(item!);
        }

        AddEvents(removed.Select(ChangeEvent.Delete).ToArray());
    }

    private IObservable<ChangeEvent<T>> CreateObservable(CancellationToken ct, ILogger logger) =>
        ObservableExt.StaticSyncInstance<ChangeEvent<T>>(
            async ctx =>
            {
                try
                {
                    while (!ctx.Ct.IsCancellationRequested)
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

    public IEnumerator<T> GetEnumerator() => Get().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Get().GetEnumerator();
}
