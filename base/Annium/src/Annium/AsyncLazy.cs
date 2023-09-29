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
    /// The underlying lazy task's value state.
    /// </summary>
    public bool IsValueCreated => _isValueCreated;

    /// <summary>
    /// The underlying lazy task's value state.
    /// </summary>
    public T Value => _instance.Value.Await();

    /// <summary>
    /// The underlying lazy task.
    /// </summary>
    private readonly Lazy<Task<T>> _instance;

    private bool _isValueCreated;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLazy&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="factory">The delegate that is invoked on a background thread to produce the value when it is needed.</param>
    public AsyncLazy(Func<T> factory)
    {
        _instance = new Lazy<Task<T>>(async () =>
        {
            var value = await Task.Run(factory).ConfigureAwait(false);
            Volatile.Write(ref _isValueCreated, true);
            return value;
        }, isThreadSafe: true);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLazy&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="factory">The asynchronous delegate that is invoked on a background thread to produce the value when it is needed.</param>
    public AsyncLazy(Func<Task<T>> factory)
    {
        _instance = new Lazy<Task<T>>(async () =>
        {
            var value = await Task.Run(factory).ConfigureAwait(false);
            Volatile.Write(ref _isValueCreated, true);
            return value;
        }, isThreadSafe: true);
    }

    /// <summary>
    /// Asynchronous infrastructure support. This method permits instances of <see cref="AsyncLazy&lt;T&gt;"/> to be await'ed.
    /// </summary>
    public TaskAwaiter<T> GetAwaiter()
    {
        return _instance.Value.GetAwaiter();
    }
}