using System;
using System.Threading.Tasks;

namespace Annium.Internal;

internal class DisposableReference<TValue> : IDisposableReference<TValue>
    where TValue : notnull
{
    public TValue Value { get; private set; }
    private readonly Func<Task> _dispose;

    public DisposableReference(
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