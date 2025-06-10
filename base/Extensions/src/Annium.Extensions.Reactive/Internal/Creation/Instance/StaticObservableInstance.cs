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
            Subscribers.Add(observer);
            if (Subscribers.Count == 1)
                Start();
        }

        return Disposable.Create(() =>
        {
            lock (Lock)
                Subscribers.Remove(observer);
        });
    }

    /// <summary>
    /// Starts the observable execution, either synchronously or asynchronously
    /// </summary>
    private void Start()
    {
        if (_isAsync)
            Task.Run(RunAsync, _ct).GetAwaiter();
        else
            RunAsync().GetAwaiter();
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
            ctx.OnCompleted();
            this.Trace("done");
        }
        catch (Exception e)
        {
            this.Trace("Error: {e}", e);
            ctx.OnError(e);
        }
    }
}
