using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Logging;

namespace Annium.Extensions.Reactive.Internal.Creation.Instance;

internal abstract class ObservableInstanceBase<T> : ILogSubject
{
    public ILogger Logger { get; }
    protected readonly HashSet<IObserver<T>> Subscribers = new();
    protected readonly object Lock = new();
    private bool _isCompleted;
    private bool _isDisposing;

    protected ObservableInstanceBase(ILogger logger)
    {
        Logger = logger;
    }

    protected ObserverContext<T> GetObserverContext(CancellationToken ct) => new(OnNext, OnError, OnCompleted, ct);

    protected void InitDisposal()
    {
        if (_isDisposing)
            throw new ObjectDisposedException(GetType().FriendlyName());
        _isDisposing = true;
    }

    private void OnNext(T value)
    {
        EnsureNotDisposing();

        var subscribers = GetSubscribersSlice();
        foreach (var observer in subscribers)
            observer.OnNext(value);
    }

    private void OnError(Exception exception)
    {
        EnsureNotDisposing();

        var subscribers = GetSubscribersSlice();
        foreach (var observer in subscribers)
            observer.OnError(exception);
    }

    private void OnCompleted()
    {
        if (_isCompleted)
        {
            this.Trace("Observable already completed");
            throw new InvalidOperationException("Observable already completed");
        }

        _isCompleted = true;

        var subscribers = GetSubscribersSlice();
        foreach (var observer in subscribers)
            observer.OnCompleted();
    }

    private void EnsureNotDisposing()
    {
        if (_isDisposing)
        {
            this.Trace("Observable is already disposing");
            throw new ObjectDisposedException(GetType().FriendlyName());
        }
    }

    private IReadOnlyCollection<IObserver<T>> GetSubscribersSlice()
    {
        lock (Lock)
            return Subscribers.ToArray();
    }
}