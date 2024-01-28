using System;
using System.Threading;

namespace Annium;

public sealed record Injected<T>
    where T : class
{
    public T Value => _value.NotNull();
    private int _isInitialized;
    private T? _value;

    public void Init(T value)
    {
        if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) != 0)
            throw new InvalidOperationException("Already initialized");

        _value = value;
    }
}
