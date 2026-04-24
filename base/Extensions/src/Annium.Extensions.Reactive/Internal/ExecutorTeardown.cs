using System;
using System.Threading.Tasks;
using Annium.Execution.Background;

namespace Annium.Extensions.Reactive.Internal;

/// <summary>
/// Shared helper for the parallel/sequential reactive operators: awaits executor disposal on a
/// background task and forwards the terminal notification (<see cref="IObserver{T}.OnCompleted"/>
/// or <see cref="IObserver{T}.OnError"/>) to the downstream observer.
/// </summary>
internal static class ExecutorTeardown
{
    /// <summary>
    /// Disposes the executor on a background task, then calls <see cref="IObserver{T}.OnCompleted"/>.
    /// If the disposal throws a non-cancellation exception, forwards it via <see cref="IObserver{T}.OnError"/>.
    /// </summary>
    /// <typeparam name="T">The observed sequence element type</typeparam>
    /// <param name="executor">The executor to dispose</param>
    /// <param name="observer">The observer to notify of terminal state</param>
    public static void CompleteInBackground<T>(IExecutor executor, IObserver<T> observer)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await executor.DisposeAsync();
                observer.OnCompleted();
            }
            catch (OperationCanceledException)
            {
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
        });
    }
}
