using System;
using System.Threading;

namespace Annium;

public sealed record Injected<T>
{
    public T Value
    {
        get
        {
            if (_isInitialized == 0)
                throw new InvalidOperationException($"{typeof(T).FriendlyName()} is not initialized");

            return _value;
        }
    }

    private int _isInitialized;
    private T _value = default!;

    public void Init(T value)
    {
        if (Interlocked.CompareExchange(ref _isInitialized, 1, 0) != 0)
            throw new InvalidOperationException($"{typeof(T).FriendlyName()} is already initialized");

        _value = value;
    }
}
