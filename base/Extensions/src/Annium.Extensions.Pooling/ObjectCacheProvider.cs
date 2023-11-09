using System;
using System.Threading;
using System.Threading.Tasks;
using OneOf;

namespace Annium.Extensions.Pooling;

public abstract class ObjectCacheProvider<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    public virtual Task<OneOf<TValue, IDisposableReference<TValue>>> CreateAsync(TKey key, CancellationToken ct) =>
        throw new NotImplementedException();

    public virtual Task SuspendAsync(TKey key, TValue value) => Task.CompletedTask;

    public virtual Task ResumeAsync(TKey key, TValue value) => Task.CompletedTask;

    public virtual Task DisposeAsync(TKey key, TValue value) => Task.CompletedTask;
}
