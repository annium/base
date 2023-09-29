using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Pooling;

public static class CacheReference
{
    public static ICacheReference<TValue> Create<TValue>(TValue value)
        where TValue : notnull
    {
        return new CacheReference<TValue>(value, () => Task.CompletedTask);
    }

    public static ICacheReference<TValue> Create<TValue>(TValue value, Func<Task> dispose)
        where TValue : notnull
    {
        return new CacheReference<TValue>(value, dispose);
    }
}

internal class CacheReference<TValue> : ICacheReference<TValue>
    where TValue : notnull
{
    public TValue Value { get; private set; }
    private readonly Func<Task> _dispose;

    public CacheReference(
        TValue value,
        Func<Task> dispose
    )
    {
        Value = value;
        _dispose = dispose;
    }

    public async ValueTask DisposeAsync()
    {
        Value = default!;
        await _dispose();
    }
}