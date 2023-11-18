using System.Threading;
using System.Threading.Tasks;
using OneOf;

namespace Annium.Extensions.Pooling;

public abstract class ObjectCacheProvider<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    public abstract Task<OneOf<TValue, IDisposableReference<TValue>>> CreateAsync(TKey key, CancellationToken ct);

    public virtual Task SuspendAsync(TKey key, TValue value) => Task.CompletedTask;

    public virtual Task ResumeAsync(TKey key, TValue value) => Task.CompletedTask;

    public virtual Task DisposeAsync(TKey key, TValue value) => Task.CompletedTask;
}
