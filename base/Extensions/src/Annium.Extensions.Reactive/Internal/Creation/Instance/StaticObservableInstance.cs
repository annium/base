using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Reactive.Internal.Creation.Instance;

internal class StaticObservableInstance<T> : ObservableInstanceBase<T>, IObservable<T>
{
    private readonly Func<ObserverContext<T>, Task<Func<Task>>> _factory;
    private readonly bool _isAsync;
    private readonly CancellationToken _ct;

    internal StaticObservableInstance(
        Func<ObserverContext<T>, Task<Func<Task>>> factory,
        bool isAsync,
        CancellationToken ct,
        ILogger logger
    ) : base(logger)
    {
        _factory = factory;
        _isAsync = isAsync;
        _ct = ct;
    }

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

    private void Start()
    {
        if (_isAsync)
            Task.Run(RunAsync, _ct);
        else
            RunAsync().GetAwaiter();
    }

    private async Task RunAsync(
    )
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