using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Threading.Tasks;

namespace Annium;

/// <summary>
/// Provides support for asynchronous lazy initialization. This type is fully threadsafe.
/// </summary>
/// <typeparam name="T">The type of object that is being asynchronously initialized.</typeparam>
public sealed class AsyncLazy<T>
{
    /// <summary>
    /// Gets a value indicating whether the value has been created.
    /// </summary>
    public bool IsValueCreated => _isValueCreated;

    /// <summary>
    /// Gets the value of the asynchronous operation.
    /// </summary>
    public T Value => _instance.Value.Await();

    /// <summary>
    /// The underlying lazy task.
    /// </summary>
    private readonly Lazy<Task<T>> _instance;

    /// <summary>
    /// Indicates whether the value has been created.
    /// </summary>
    private bool _isValueCreated;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class with a synchronous factory.
    /// </summary>
    /// <param name="factory">The delegate that is invoked on a background thread to produce the value when it is needed.</param>
    public AsyncLazy(Func<T> factory)
    {
#pragma warning disable VSTHRD011
        _instance = new Lazy<Task<T>>(
#pragma warning restore VSTHRD011
            async () =>
            {
                var value = await Task.Run(factory).ConfigureAwait(false);
                Volatile.Write(ref _isValueCreated, true);
                return value;
            },
            isThreadSafe: true
        );
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class with an asynchronous factory.
    /// </summary>
    /// <param name="factory">The asynchronous delegate that is invoked on a background thread to produce the value when it is needed.</param>
    public AsyncLazy(Func<Task<T>> factory)
    {
#pragma warning disable VSTHRD011
        _instance = new Lazy<Task<T>>(
#pragma warning restore VSTHRD011
            async () =>
            {
                var value = await Task.Run(factory).ConfigureAwait(false);
                Volatile.Write(ref _isValueCreated, true);
                return value;
            },
            isThreadSafe: true
        );
    }

    /// <summary>
    /// Gets the awaiter for the asynchronous operation.
    /// </summary>
    /// <returns>A <see cref="TaskAwaiter{T}"/> for the asynchronous operation.</returns>
    public TaskAwaiter<T> GetAwaiter()
    {
        return _instance.Value.GetAwaiter();
    }
}
