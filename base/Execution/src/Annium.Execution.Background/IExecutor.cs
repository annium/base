using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Execution.Background;

/// <summary>
/// Represents a background task executor interface
/// </summary>
public interface IExecutor : IAsyncDisposable
{
    /// <summary>
    /// Gets a value indicating whether the executor is available to schedule new tasks
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Schedules a synchronous task for execution
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <returns>True if the task was successfully scheduled, false otherwise</returns>
    bool Schedule(Action task);

    /// <summary>
    /// Schedules a synchronous task for execution with cancellation support
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <returns>True if the task was successfully scheduled, false otherwise</returns>
    bool Schedule(Action<CancellationToken> task);

    /// <summary>
    /// Schedules an asynchronous task for execution
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <returns>True if the task was successfully scheduled, false otherwise</returns>
    bool Schedule(Func<ValueTask> task);

    /// <summary>
    /// Schedules an asynchronous task for execution with cancellation support
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <returns>True if the task was successfully scheduled, false otherwise</returns>
    bool Schedule(Func<CancellationToken, ValueTask> task);

    /// <summary>
    /// Starts the executor
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The executor instance</returns>
    IExecutor Start(CancellationToken ct = default);
}
