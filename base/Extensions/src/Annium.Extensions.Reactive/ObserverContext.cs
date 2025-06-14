using System.Threading;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Represents an observer context that wraps observer callbacks with a cancellation token
/// </summary>
/// <typeparam name="T">The type of items observed</typeparam>
public sealed record ObserverContext<T> : IObserver<T>
{
    /// <summary>
    /// Callback for handling next values
    /// </summary>
    private readonly Action<T> _onNext;

    /// <summary>
    /// Callback for handling errors
    /// </summary>
    private readonly Action<Exception> _onError;

    /// <summary>
    /// Callback for handling completion
    /// </summary>
    private readonly Action _onCompleted;

    /// <summary>
    /// Gets the cancellation token associated with this observer context
    /// </summary>
    public CancellationToken Ct { get; }

    /// <summary>
    /// Initializes a new instance of the ObserverContext class
    /// </summary>
    /// <param name="onNext">Callback to invoke when a new value is observed</param>
    /// <param name="onError">Callback to invoke when an error occurs</param>
    /// <param name="onCompleted">Callback to invoke when the observable completes</param>
    /// <param name="ct">Cancellation token for the observation</param>
    public ObserverContext(Action<T> onNext, Action<Exception> onError, Action onCompleted, CancellationToken ct)
    {
        _onNext = onNext;
        _onError = onError;
        _onCompleted = onCompleted;
        Ct = ct;
    }

    /// <summary>
    /// Notifies the observer of a new value
    /// </summary>
    /// <param name="value">The value to observe</param>
    public void OnNext(T value) => _onNext(value);

    /// <summary>
    /// Notifies the observer of an error
    /// </summary>
    /// <param name="error">The error that occurred</param>
    public void OnError(Exception error) => _onError(error);

    /// <summary>
    /// Notifies the observer that the observable has completed
    /// </summary>
    public void OnCompleted() => _onCompleted();
}
