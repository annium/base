using System;

namespace Annium.Extensions.Reactive.Internal;

internal readonly struct ObservableEvent<T>
{
    public T Data { get; }
    public Exception? Error { get; }
    public bool IsCompleted { get; }

    public ObservableEvent(
        T data,
        Exception? error,
        bool isCompleted
    )
    {
        Data = data;
        Error = error;
        IsCompleted = isCompleted;
    }
}