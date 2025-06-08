using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Jobs;

/// <summary>
/// Interface for scheduling recurring tasks using cron expressions
/// </summary>
public interface IScheduler
{
    /// <summary>
    /// Schedules a task to run at specified intervals
    /// </summary>
    /// <param name="handler">The task to execute</param>
    /// <param name="interval">The cron expression defining the schedule</param>
    /// <returns>A disposable to cancel the scheduled task</returns>
    IDisposable Schedule(Func<Task> handler, string interval);
}
