using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution;

public static class BackgroundExecutorExtensions
{
    public static async ValueTask ExecuteAsync(this IBackgroundExecutor executor, Action task)
    {
        var tcs = new TaskCompletionSource();
        executor.Schedule(() =>
        {
            try
            {
                task();
                tcs.SetResult();
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

        await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask ExecuteAsync(this IBackgroundExecutor executor, Action<CancellationToken> task)
    {
        var tcs = new TaskCompletionSource();
        executor.Schedule(ct =>
        {
            try
            {
                task(ct);
                tcs.SetResult();
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

        await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask<T> ExecuteAsync<T>(this IBackgroundExecutor executor, Func<T> task)
    {
        var tcs = new TaskCompletionSource<T>();
        executor.Schedule(() =>
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

        return await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask<T> ExecuteAsync<T>(this IBackgroundExecutor executor, Func<CancellationToken, T> task)
    {
        var tcs = new TaskCompletionSource<T>();
        executor.Schedule(ct =>
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

        return await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask ExecuteAsync(this IBackgroundExecutor executor, Func<ValueTask> task)
    {
        var tcs = new TaskCompletionSource();
        executor.Schedule(async () =>
        {
            try
            {
                await task();
                tcs.SetResult();
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

        await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask ExecuteAsync(this IBackgroundExecutor executor, Func<CancellationToken, ValueTask> task)
    {
        var tcs = new TaskCompletionSource();
        executor.Schedule(async ct =>
        {
            try
            {
                await task(ct);
                tcs.SetResult();
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

        await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask<T> ExecuteAsync<T>(this IBackgroundExecutor executor, Func<ValueTask<T>> task)
    {
        var tcs = new TaskCompletionSource<T>();
        executor.Schedule(async () =>
        {
            try
            {
                tcs.SetResult(await task());
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

        return await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask<T> ExecuteAsync<T>(this IBackgroundExecutor executor, Func<CancellationToken, ValueTask<T>> task)
    {
        var tcs = new TaskCompletionSource<T>();
        executor.Schedule(async ct =>
        {
            try
            {
                tcs.SetResult(await task(ct));
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

        return await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask TryExecuteAsync(this IBackgroundExecutor executor, Action task)
    {
        var tcs = new TaskCompletionSource();
        var scheduled = executor.TrySchedule(() =>
        {
            try
            {
                task();
                tcs.SetResult();
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
            tcs.SetException(UnavailableException());

        await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask TryExecuteAsync(this IBackgroundExecutor executor, Action<CancellationToken> task)
    {
        var tcs = new TaskCompletionSource();
        var scheduled = executor.TrySchedule(ct =>
        {
            try
            {
                task(ct);
                tcs.SetResult();
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
            tcs.SetException(UnavailableException());

        await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask<T> TryExecuteAsync<T>(this IBackgroundExecutor executor, Func<T> task)
    {
        var tcs = new TaskCompletionSource<T>();
        var scheduled = executor.TrySchedule(() =>
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
            tcs.SetException(UnavailableException());

        return await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask<T> TryExecuteAsync<T>(this IBackgroundExecutor executor, Func<CancellationToken, T> task)
    {
        var tcs = new TaskCompletionSource<T>();
        var scheduled = executor.TrySchedule(ct =>
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
            tcs.SetException(UnavailableException());

        return await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask TryExecuteAsync(this IBackgroundExecutor executor, Func<ValueTask> task)
    {
        var tcs = new TaskCompletionSource();
        var scheduled = executor.TrySchedule(async () =>
        {
            try
            {
                await task().ConfigureAwait(false);
                tcs.SetResult();
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
            tcs.SetException(UnavailableException());

        await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask TryExecuteAsync(this IBackgroundExecutor executor, Func<CancellationToken, ValueTask> task)
    {
        var tcs = new TaskCompletionSource();
        var scheduled = executor.TrySchedule(async ct =>
        {
            try
            {
                await task(ct).ConfigureAwait(false);
                tcs.SetResult();
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
            tcs.SetException(UnavailableException());

        await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask<T> TryExecuteAsync<T>(this IBackgroundExecutor executor, Func<ValueTask<T>> task)
    {
        var tcs = new TaskCompletionSource<T>();
        var scheduled = executor.TrySchedule(async () =>
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
            tcs.SetException(UnavailableException());

        return await tcs.Task.ConfigureAwait(false);
    }

    public static async ValueTask<T> TryExecuteAsync<T>(this IBackgroundExecutor executor, Func<CancellationToken, ValueTask<T>> task)
    {
        var tcs = new TaskCompletionSource<T>();
        var scheduled = executor.TrySchedule(async ct =>
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
            tcs.SetException(UnavailableException());

        return await tcs.Task.ConfigureAwait(false);
    }


    private static InvalidOperationException UnavailableException() =>
        new("Executor is not available already");
}