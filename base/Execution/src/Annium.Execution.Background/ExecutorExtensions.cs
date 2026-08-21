using System;
using System.Threading;
using System.Threading.Tasks;
using OneOf;

namespace Annium.Execution.Background;

/// <summary>
/// Extension methods for IExecutor to provide convenient execution patterns
/// </summary>
public static class ExecutorExtensions
{
    /// <summary>
    /// Options for the result-relay <see cref="TaskCompletionSource"/>s: continuations resume on the
    /// thread pool rather than inline on the executor task that completes the source, so the awaiting
    /// caller never runs on (and blocks) the executor's own loop.
    /// </summary>
    private const TaskCreationOptions TcsOptions = TaskCreationOptions.RunContinuationsAsynchronously;

    /// <summary>
    /// Executes a synchronous task and waits for completion
    /// </summary>
    /// <param name="executor">The executor instance</param>
    /// <param name="task">The task to execute</param>
    /// <returns>True if the task was executed successfully, false if it could not be scheduled</returns>
    public static async ValueTask<bool> ExecuteAsync(this IExecutor executor, Action task)
    {
        var tcs = new TaskCompletionSource<bool>(TcsOptions);
        var scheduled = executor.Schedule(() =>
        {
            try
            {
                task();
                tcs.SetResult(true);
            }
            catch (OperationCanceledException)
            {
                tcs.SetCanceled(CancellationToken.None);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (!scheduled)
            tcs.SetResult(false);

        return await tcs.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a synchronous task with cancellation support and waits for completion
    /// </summary>
    /// <param name="executor">The executor instance</param>
    /// <param name="task">The task to execute</param>
    /// <returns>True if the task was executed successfully, false if it could not be scheduled</returns>
    public static async ValueTask<bool> ExecuteAsync(this IExecutor executor, Action<CancellationToken> task)
    {
        var tcs = new TaskCompletionSource<bool>(TcsOptions);
        var scheduled = executor.Schedule(ct =>
        {
            try
            {
                task(ct);
                tcs.SetResult(true);
            }
            catch (OperationCanceledException)
            {
                tcs.SetCanceled(ct);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (!scheduled)
            tcs.SetResult(false);

        return await tcs.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a synchronous function and waits for completion
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the function.</typeparam>
    /// <param name="executor">The executor instance</param>
    /// <param name="task">The function to execute</param>
    /// <returns>The result of the function or None if it could not be scheduled</returns>
    public static async ValueTask<OneOf<T, None>> ExecuteAsync<T>(this IExecutor executor, Func<T> task)
    {
        var tcs = new TaskCompletionSource<OneOf<T, None>>(TcsOptions);
        var scheduled = executor.Schedule(() =>
        {
            try
            {
                tcs.SetResult(task());
            }
            catch (OperationCanceledException)
            {
                tcs.SetCanceled(CancellationToken.None);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (!scheduled)
            tcs.SetResult(None.Default);

        return await tcs.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a synchronous function with cancellation support and waits for completion
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the function.</typeparam>
    /// <param name="executor">The executor instance</param>
    /// <param name="task">The function to execute</param>
    /// <returns>The result of the function or None if it could not be scheduled</returns>
    public static async ValueTask<OneOf<T, None>> ExecuteAsync<T>(
        this IExecutor executor,
        Func<CancellationToken, T> task
    )
    {
        var tcs = new TaskCompletionSource<OneOf<T, None>>(TcsOptions);
        var scheduled = executor.Schedule(ct =>
        {
            try
            {
                tcs.SetResult(task(ct));
            }
            catch (OperationCanceledException)
            {
                tcs.SetCanceled(ct);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (!scheduled)
            tcs.SetResult(None.Default);

        return await tcs.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an asynchronous task and waits for completion
    /// </summary>
    /// <param name="executor">The executor instance</param>
    /// <param name="task">The task to execute</param>
    /// <returns>True if the task was executed successfully, false if it could not be scheduled</returns>
    public static async ValueTask<bool> ExecuteAsync(this IExecutor executor, Func<ValueTask> task)
    {
        var tcs = new TaskCompletionSource<bool>(TcsOptions);
        var scheduled = executor.Schedule(async () =>
        {
            try
            {
                await task().ConfigureAwait(false);
                tcs.SetResult(true);
            }
            catch (OperationCanceledException)
            {
                tcs.SetCanceled(CancellationToken.None);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (!scheduled)
            tcs.SetResult(false);

        return await tcs.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an asynchronous task with cancellation support and waits for completion
    /// </summary>
    /// <param name="executor">The executor instance</param>
    /// <param name="task">The task to execute</param>
    /// <returns>True if the task was executed successfully, false if it could not be scheduled</returns>
    public static async ValueTask<bool> ExecuteAsync(this IExecutor executor, Func<CancellationToken, ValueTask> task)
    {
        var tcs = new TaskCompletionSource<bool>(TcsOptions);
        var scheduled = executor.Schedule(async ct =>
        {
            try
            {
                await task(ct).ConfigureAwait(false);
                tcs.SetResult(true);
            }
            catch (OperationCanceledException)
            {
                tcs.SetCanceled(ct);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (!scheduled)
            tcs.SetResult(false);

        return await tcs.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an asynchronous function and waits for completion
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the function.</typeparam>
    /// <param name="executor">The executor instance</param>
    /// <param name="task">The function to execute</param>
    /// <returns>The result of the function or None if it could not be scheduled</returns>
    public static async ValueTask<OneOf<T, None>> ExecuteAsync<T>(this IExecutor executor, Func<ValueTask<T>> task)
    {
        var tcs = new TaskCompletionSource<OneOf<T, None>>(TcsOptions);
        var scheduled = executor.Schedule(async () =>
        {
            try
            {
                tcs.SetResult(await task().ConfigureAwait(false));
            }
            catch (OperationCanceledException)
            {
                tcs.SetCanceled(CancellationToken.None);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (!scheduled)
            tcs.SetResult(None.Default);

        return await tcs.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an asynchronous function with cancellation support and waits for completion
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the function.</typeparam>
    /// <param name="executor">The executor instance</param>
    /// <param name="task">The function to execute</param>
    /// <returns>The result of the function or None if it could not be scheduled</returns>
    public static async ValueTask<OneOf<T, None>> ExecuteAsync<T>(
        this IExecutor executor,
        Func<CancellationToken, ValueTask<T>> task
    )
    {
        var tcs = new TaskCompletionSource<OneOf<T, None>>(TcsOptions);
        var scheduled = executor.Schedule(async ct =>
        {
            try
            {
                tcs.SetResult(await task(ct).ConfigureAwait(false));
            }
            catch (OperationCanceledException)
            {
                tcs.SetCanceled(ct);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (!scheduled)
            tcs.SetResult(None.Default);

        return await tcs.Task.ConfigureAwait(false);
    }
}
