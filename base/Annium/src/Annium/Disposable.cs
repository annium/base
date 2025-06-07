using System;
using System.Threading.Tasks;
using Annium.Internal;
using Annium.Logging;

namespace Annium;

/// <summary>
/// Provides factory methods for creating disposable resources and containers.
/// </summary>
public static class Disposable
{
    /// <summary>
    /// Creates a new synchronous disposable box.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    /// <returns>A new <see cref="DisposableBox"/> instance.</returns>
    public static DisposableBox Box(ILogger logger) => new(logger);

    /// <summary>
    /// Creates a new asynchronous disposable box.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    /// <returns>A new <see cref="AsyncDisposableBox"/> instance.</returns>
    public static AsyncDisposableBox AsyncBox(ILogger logger) => new(logger);

    /// <summary>
    /// Creates an asynchronous disposable resource from a dispose function.
    /// </summary>
    /// <param name="handle">The asynchronous function to execute when disposing.</param>
    /// <returns>An <see cref="IAsyncDisposable"/> that executes the specified function when disposed.</returns>
    public static IAsyncDisposable Create(Func<Task> handle) => new AsyncDisposer(handle);

    /// <summary>
    /// Creates a synchronous disposable resource from a dispose action.
    /// </summary>
    /// <param name="handle">The action to execute when disposing.</param>
    /// <returns>An <see cref="IDisposable"/> that executes the specified action when disposed.</returns>
    public static IDisposable Create(Action handle) => new Disposer(handle);

    /// <summary>
    /// Gets an empty disposable resource that does nothing when disposed.
    /// </summary>
    public static readonly IDisposable Empty = new EmptyDisposer();

    /// <summary>
    /// Creates a disposable reference to a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to reference.</typeparam>
    /// <param name="value">The value to reference.</param>
    /// <returns>A disposable reference to the specified value.</returns>
    public static IDisposableReference<TValue> Reference<TValue>(TValue value)
        where TValue : notnull
    {
        return new DisposableReference<TValue>(value, () => Task.CompletedTask);
    }

    /// <summary>
    /// Creates a disposable reference to a value with a custom dispose function.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to reference.</typeparam>
    /// <param name="value">The value to reference.</param>
    /// <param name="dispose">The asynchronous function to execute when disposing.</param>
    /// <returns>A disposable reference to the specified value.</returns>
    public static IDisposableReference<TValue> Reference<TValue>(TValue value, Func<Task> dispose)
        where TValue : notnull
    {
        return new DisposableReference<TValue>(value, dispose);
    }
}
