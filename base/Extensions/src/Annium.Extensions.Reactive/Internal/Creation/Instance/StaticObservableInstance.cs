using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Reactive.Internal.Creation.Instance;

/// <summary>
/// A static observable instance that executes a factory function to produce values for subscribers
/// </summary>
/// <typeparam name="T">The type of items emitted by the observable</typeparam>
internal class StaticObservableInstance<T> : ObservableInstanceBase<T>, IObservable<T>
{
    /// <summary>
    /// Factory function that produces values for the observable
    /// </summary>
    private readonly Func<ObserverContext<T>, Task<Func<Task>>> _factory;

    /// <summary>
    /// Indicates whether the factory should run asynchronously
    /// </summary>
    private readonly bool _isAsync;

    /// <summary>
    /// Cancellation token for the observable operation
    /// </summary>
    private readonly CancellationToken _ct;

    /// <summary>
    /// Whether the factory has been started. It runs once per instance
    /// </summary>
    private bool _isStarted;

    /// <summary>
    /// Whether the factory has finished, however it finished
    /// </summary>
    private bool _isEnded;

    /// <summary>
    /// The failure the run ended with, or null if it completed normally
    /// </summary>
    private Exception? _error;

    /// <summary>
    /// Initializes a new instance of the StaticObservableInstance class
    /// </summary>
    /// <param name="factory">Factory function that produces an async disposal function</param>
    /// <param name="isAsync">Whether to run the factory asynchronously</param>
    /// <param name="ct">Cancellation token for the operation</param>
    /// <param name="logger">Logger instance for this observable</param>
    internal StaticObservableInstance(
        Func<ObserverContext<T>, Task<Func<Task>>> factory,
        bool isAsync,
        CancellationToken ct,
        ILogger logger
    )
        : base(logger)
    {
        _factory = factory;
        _isAsync = isAsync;
        _ct = ct;
    }

    /// <summary>
    /// Subscribes an observer to this observable
    /// </summary>
    /// <param name="observer">The observer to subscribe</param>
    /// <returns>A disposable that can be used to unsubscribe the observer</returns>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        lock (Lock)
        {
            if (!_isEnded)
            {
                Subscribers.Add(observer);

                // once per instance, not once per time the subscriber count passes through one: starting
                // again after the first run reran the factory over the first run's disposal state, which
                // failed immediately and left the new subscriber with nothing at all
                if (!_isStarted)
                {
                    _isStarted = true;
                    Start();
                }

                return Disposable.Create(() =>
                {
                    lock (Lock)
                        Subscribers.Remove(observer);
                });
            }
        }

        // there is nothing left to join. Saying so beats attaching to a source that will never speak again -
        // and it has to be what actually happened: replaying a completion over a failure swallows it just
        // as thoroughly as replaying nothing at all
        var error = Volatile.Read(ref _error);
        this.Trace("already ended - terminate subscriber at once");
        if (error is null)
            observer.OnCompleted();
        else
            observer.OnError(error);

        return Disposable.Empty;
    }

    /// <summary>
    /// Starts the observable execution, either synchronously or asynchronously.
    /// <see cref="RunAsync"/> contains its own try/catch that surfaces failures via
    /// <c>ctx.OnError</c>, so the discarded task handle cannot silently drop an exception.
    /// </summary>
    private void Start()
    {
        if (_isAsync)
            _ = Task.Run(RunAsync, _ct);
        else
            _ = RunAsync();
    }

    /// <summary>
    /// Executes the factory function and handles the observable lifecycle
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task RunAsync()
    {
        var ctx = GetObserverContext(_ct);
        try
        {
            this.Trace("start, run factory");
            var disposeAsync = await _factory(ctx);
            this.Trace("init disposal");
            InitDisposal();
            this.Trace("dispose");
            await disposeAsync();
            this.Trace("invoke onCompleted");
            End(null);
            ctx.OnCompleted();
            this.Trace("done");
        }
        catch (Exception e)
        {
            this.Trace("Error: {e}", e);
            End(e);
            ctx.OnError(e);
        }
    }

    /// <summary>
    /// Marks the run as finished, before the terminal notification rather than after it: a subscriber
    /// arriving while that notification is being delivered would otherwise be added to a run that has
    /// nothing left to say
    /// </summary>
    /// <param name="error">The failure the run ended with, or null if it completed normally</param>
    private void End(Exception? error)
    {
        lock (Lock)
        {
            _error = error;
            _isEnded = true;
        }
    }
}
