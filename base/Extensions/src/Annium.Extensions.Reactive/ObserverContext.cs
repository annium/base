using System.Threading;

// ReSharper disable once CheckNamespace
namespace System;

public sealed record ObserverContext<T> : IObserver<T>
{
    private readonly Action<T> _onNext;
    private readonly Action<Exception> _onError;
    private readonly Action _onCompleted;

    public CancellationToken Ct { get; }

    public ObserverContext(
        Action<T> onNext,
        Action<Exception> onError,
        Action onCompleted,
        CancellationToken ct
    )
    {
        _onNext = onNext;
        _onError = onError;
        _onCompleted = onCompleted;
        Ct = ct;
    }

    public void OnNext(T value) => _onNext(value);

    public void OnError(Exception error) => _onError(error);

    public void OnCompleted() => _onCompleted();
}