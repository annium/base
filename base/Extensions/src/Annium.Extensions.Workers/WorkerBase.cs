using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Workers;

public abstract class WorkerBase<TKey> : IAsyncDisposable
    where TKey : IEquatable<TKey>
{
    protected TKey Key { get; private set; } = default!;
    private readonly CancellationTokenSource _cts = new();

    public ValueTask InitAsync(TKey key)
    {
        Key = key;
        return StartAsync(_cts.Token);
    }

    protected abstract ValueTask StartAsync(CancellationToken сt);
    protected abstract ValueTask StopAsync();

    public ValueTask DisposeAsync()
    {
        _cts.Cancel();
        return StopAsync();
    }
}
