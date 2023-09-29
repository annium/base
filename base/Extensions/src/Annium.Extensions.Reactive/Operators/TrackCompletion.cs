using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using Annium;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

public static class TrackCompletionOperatorExtensions
{
    public static IObservable<T> TrackCompletion<T>(
        this IObservable<T> source,
        ILogger logger
    )
    {
        var ctx = new CompletionContext<T>(source, logger);

        source.Subscribe(delegate { }, ctx.Complete, ctx.CompletionCt);

        ctx.Trace("create observable");

        return Observable.Create<T>(observer =>
        {
            var target = observer.GetFullId();
            ctx.Trace<string>("{target} - handle", target);

            if (!ctx.IsCompleted)
                return ctx.Subscribe(observer);

            ctx.Trace<string>("{target} - complete", target);
            observer.OnCompleted();
            ctx.Trace<string>("{target} - completed", target);

            return Disposable.Empty;
        });
    }
}

file record CompletionContext<T> : ILogSubject
{
    public bool IsCompleted { get; private set; }
    public ILogger Logger { get; }
    public CancellationToken CompletionCt => _completionCts.Token;
    private readonly IObservable<T> _source;
    private readonly List<IObserver<T>> _incompleteObservers = new();
    private readonly CancellationTokenSource _completionCts = new();

    public CompletionContext(
        IObservable<T> source,
        ILogger logger
    )
    {
        Logger = logger;
        _source = source;
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        lock (this)
        {
            this.Trace("start");

            _incompleteObservers.Add(observer);
            var subscription = _source.Subscribe(observer.OnNext, observer.OnError);

            this.Trace("done");

            return subscription;
        }
    }

    public void Complete()
    {
        this.Trace("start");

        IReadOnlyCollection<IObserver<T>> observers;
        lock (this)
        {
            if (IsCompleted)
                throw new InvalidOperationException("source already completed");
            IsCompleted = true;

            this.Trace("complete {incompleteObserversCount} observers", _incompleteObservers.Count);
            observers = _incompleteObservers.ToArray();
            _incompleteObservers.Clear();
        }

        foreach (var observer in observers)
        {
            this.Trace<string>("complete {observer}", observer.GetFullId());
            observer.OnCompleted();
        }

        this.Trace("cancel cts");
        _completionCts.Cancel();

        this.Trace("done");
    }
}