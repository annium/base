using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Workers;

public abstract class WorkerBase<TKey> : IAsyncDisposable
    where TKey : IEquatable<TKey>
{
    protected TKey Key { get; private set; } = default!;

    public ValueTask InitAsync(TKey key)
    {
        Key = key;
        return InitAsync();
    }

    public abstract ValueTask InitAsync();
    public abstract ValueTask DisposeAsync();
}
