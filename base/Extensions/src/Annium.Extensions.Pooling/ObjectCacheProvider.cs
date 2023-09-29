using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Pooling;

public abstract class ObjectCacheProvider<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    public abstract bool HasCreate { get; }
    public abstract bool HasExternalCreate { get; }
    public virtual Task<TValue> CreateAsync(TKey key, CancellationToken ct) => throw new NotImplementedException();
    public virtual Task<ICacheReference<TValue>> ExternalCreateAsync(TKey key, CancellationToken ct) => throw new NotImplementedException();
    public virtual Task SuspendAsync(TValue value) => Task.CompletedTask;
    public virtual Task ResumeAsync(TValue value) => Task.CompletedTask;
}