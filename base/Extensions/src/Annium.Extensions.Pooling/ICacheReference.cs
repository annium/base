using System;

namespace Annium.Extensions.Pooling;

public interface ICacheReference<out TValue> : IAsyncDisposable
    where TValue : notnull
{
    TValue Value { get; }
}