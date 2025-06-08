using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Annium.Logging;

namespace Annium.Extensions.Reactive.Internal.Creation.Instance;

/// <summary>
/// Base class for custom observable instances that provides common functionality for managing subscribers and state
/// </summary>
/// <typeparam name="T">The type of items emitted by the observable</typeparam>
internal abstract class ObservableInstanceBase<T> : ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this observable
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Collection of subscribers to this observable
    /// </summary>
    protected readonly HashSet<IObserver<T>> Subscribers = new();

    /// <summary>
    /// Synchronization object for thread-safe access to subscribers
    /// </summary>
    protected readonly object Lock = new();

    /// <summary>
    /// Indicates whether the observable has completed
    /// </summary>
    private bool _isCompleted;

    /// <summary>
    /// Indicates whether the observable is in the process of disposing
    /// </summary>
    private bool _isDisposing;

    /// <summary>
    /// Initializes a new instance of the ObservableInstanceBase class
    /// </summary>
    /// <param name="logger">The logger to use for this observable instance</param>
    protected ObservableInstanceBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Creates an observer context with the provided cancellation token
    /// </summary>
    /// <param name="ct">The cancellation token to associate with the observer context</param>
    /// <returns>An observer context that can be used to emit values, errors, and completion signals</returns>
    protected ObserverContext<T> GetObserverContext(CancellationToken ct) => new(OnNext, OnError, OnCompleted, ct);

    /// <summary>
    /// Initializes the disposal process for this observable instance
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the observable is already disposing</exception>
    protected void InitDisposal()
    {
        if (_isDisposing)
            throw new ObjectDisposedException(GetType().FriendlyName());
        _isDisposing = true;
    }

    /// <summary>
    /// Emits a value to all current subscribers
    /// </summary>
    /// <param name="value">The value to emit</param>
    private void OnNext(T value)
    {
        EnsureNotDisposing();

        var subscribers = GetSubscribersSlice();
        foreach (var observer in subscribers)
            observer.OnNext(value);
    }

    /// <summary>
    /// Emits an error to all current subscribers
    /// </summary>
    /// <param name="exception">The exception to emit</param>
    private void OnError(Exception exception)
    {
        EnsureNotDisposing();

        var subscribers = GetSubscribersSlice();
        foreach (var observer in subscribers)
            observer.OnError(exception);
    }

    /// <summary>
    /// Signals completion to all current subscribers
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the observable is already completed</exception>
    private void OnCompleted()
    {
        if (_isCompleted)
        {
            this.Trace("Observable already completed");
            throw new InvalidOperationException("Observable already completed");
        }

        _isCompleted = true;

        var subscribers = GetSubscribersSlice();
        foreach (var observer in subscribers)
            observer.OnCompleted();
    }

    /// <summary>
    /// Ensures the observable is not in the process of disposing
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the observable is disposing</exception>
    private void EnsureNotDisposing()
    {
        if (_isDisposing)
        {
            this.Trace("Observable is already disposing");
            throw new ObjectDisposedException(GetType().FriendlyName());
        }
    }

    /// <summary>
    /// Gets a thread-safe snapshot of current subscribers
    /// </summary>
    /// <returns>A read-only collection containing the current subscribers</returns>
    private IReadOnlyCollection<IObserver<T>> GetSubscribersSlice()
    {
        lock (Lock)
            return Subscribers.ToArray();
    }
}
