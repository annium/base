using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using Annium;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Provides operators for tracking completion state of observables
/// </summary>
public static class TrackCompletionOperatorExtensions
{
    /// <summary>
    /// Tracks the completion state of an observable, allowing late subscribers to receive completion immediately if the source has already completed
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the observable</typeparam>
    /// <param name="source">The source observable to track</param>
    /// <param name="logger">Logger for tracking completion events</param>
    /// <returns>An observable that tracks and replays completion state</returns>
    public static IObservable<T> TrackCompletion<T>(this IObservable<T> source, ILogger logger)
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

/// <summary>
/// Context for tracking completion state of an observable and managing subscribers
/// </summary>
/// <typeparam name="T">The type of items emitted by the observable</typeparam>
file record CompletionContext<T> : ILogSubject
{
    /// <summary>
    /// Gets a value indicating whether the source observable has completed
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Gets the logger instance for this completion context
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the cancellation token that is signaled when completion occurs
    /// </summary>
    public CancellationToken CompletionCt => _completionCts.Token;

    /// <summary>
    /// The source observable being tracked
    /// </summary>
    private readonly IObservable<T> _source;

    /// <summary>
    /// List of observers that have not yet completed
    /// </summary>
    private readonly List<IObserver<T>> _incompleteObservers = new();

    /// <summary>
    /// Cancellation token source for signaling completion
    /// </summary>
    private readonly CancellationTokenSource _completionCts = new();

    /// <summary>
    /// Initializes a new instance of the CompletionContext record
    /// </summary>
    /// <param name="source">The source observable to track</param>
    /// <param name="logger">Logger for tracking completion events</param>
    public CompletionContext(IObservable<T> source, ILogger logger)
    {
        Logger = logger;
        _source = source;
    }

    /// <summary>
    /// Subscribes an observer to the source observable and tracks it for completion
    /// </summary>
    /// <param name="observer">The observer to subscribe</param>
    /// <returns>A disposable subscription</returns>
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

    /// <summary>
    /// Marks the observable as completed and notifies all incomplete observers
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the source is already completed</exception>
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
