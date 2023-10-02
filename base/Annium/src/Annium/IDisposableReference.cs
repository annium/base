using System;

namespace Annium;

public interface IDisposableReference<out TValue> : IAsyncDisposable
    where TValue : notnull
{
    TValue Value { get; }
}