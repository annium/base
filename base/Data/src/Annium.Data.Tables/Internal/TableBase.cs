using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Data.Tables.Internal;

internal abstract class TableBase<T> : ITableView<T>, ILogSubject
{
    public ILogger Logger { get; }
    public abstract int Count { get; }
    protected readonly object DataLocker = new();
    private readonly CancellationTokenSource _observableCts = new();
    private readonly IObservable<IChangeEvent<T>> _observable;
    private readonly TablePermission _permissions;
    private readonly ChannelWriter<IChangeEvent<T>> _eventWriter;
    private readonly ChannelReader<IChangeEvent<T>> _eventReader;

    protected TableBase(
        TablePermission permissions,
        ILogger logger
    )
    {
        _permissions = permissions;
        var taskChannel = Channel.CreateUnbounded<IChangeEvent<T>>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = true,
            SingleWriter = false,
            SingleReader = true
        });
        _eventWriter = taskChannel.Writer;
        _eventReader = taskChannel.Reader;

        _observable = CreateObservable(_observableCts.Token, logger).TrackCompletion(logger);
        Logger = logger;
    }

    public IDisposable Subscribe(IObserver<IChangeEvent<T>> observer)
    {
        var init = ChangeEvent.Init(Get());
        observer.OnNext(init);

        return _observable.Subscribe(observer);
    }

    protected void AddEvents(IReadOnlyCollection<IChangeEvent<T>> events)
    {
        foreach (var @event in events)
            if (!_eventWriter.TryWrite(@event))
                throw new InvalidOperationException("Event must have been sent.");
    }

    protected void AddEvent(IChangeEvent<T> @event)
    {
        if (!_eventWriter.TryWrite(@event))
            throw new InvalidOperationException("Event must have been sent.");
    }

    protected void EnsurePermission(TablePermission permission)
    {
        if (!_permissions.HasFlag(permission))
            throw new InvalidOperationException($"Table {GetType().FriendlyName()} has no {permission} permission.");
    }

    protected abstract IReadOnlyCollection<T> Get();

    private IObservable<IChangeEvent<T>> CreateObservable(CancellationToken ct, ILogger logger) => ObservableExt.StaticSyncInstance<IChangeEvent<T>>(async ctx =>
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
        catch (OperationCanceledException)
        {
        }
        catch (ChannelClosedException)
        {
        }
        catch (Exception e)
        {
            ctx.OnError(e);
        }

        return () => Task.CompletedTask;
    }, ct, logger);

    public IEnumerator<T> GetEnumerator() => Get().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Get().GetEnumerator();

    public virtual async ValueTask DisposeAsync()
    {
        this.Trace("start, complete writer");
        _eventWriter.Complete();
        this.Trace("cancel observable");
        _observableCts.Cancel();
        this.Trace("await observable completion");
        await _observable.WhenCompleted(Logger);
        this.Trace("done");
    }
}