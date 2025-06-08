using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;
using Annium.NodaTime.Extensions;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal;

/// <summary>
/// Implementation of job scheduler that executes tasks based on cron expressions
/// </summary>
internal class Scheduler : IScheduler, IAsyncDisposable, ILogSubject
{
    /// <summary>
    /// Logger instance for the scheduler
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Time provider for getting current time
    /// </summary>
    private readonly ITimeProvider _timeProvider;

    /// <summary>
    /// Parser for converting cron expressions to delay functions
    /// </summary>
    private readonly IIntervalParser _intervalParser;

    /// <summary>
    /// Cancellation token source for stopping all scheduled jobs
    /// </summary>
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// Flag indicating if the scheduler has been disposed
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Background executor for running scheduled tasks
    /// </summary>
    private readonly IExecutor _executor;

    /// <summary>
    /// Initializes a new instance of the Scheduler class
    /// </summary>
    /// <param name="timeProvider">Provider for getting current time</param>
    /// <param name="intervalParser">Parser for cron expressions</param>
    /// <param name="logger">Logger for the scheduler</param>
    public Scheduler(ITimeProvider timeProvider, IIntervalParser intervalParser, ILogger logger)
    {
        Logger = logger;
        _timeProvider = timeProvider;
        _intervalParser = intervalParser;

        _executor = Executor.Parallel<Scheduler>(logger).Start();
    }

    /// <summary>
    /// Schedules a task to run at intervals specified by the cron expression
    /// </summary>
    /// <param name="handler">The async task to execute on schedule</param>
    /// <param name="interval">The cron expression defining when to run the task</param>
    /// <returns>A disposable that can be used to cancel the scheduled task</returns>
    public IDisposable Schedule(Func<Task> handler, string interval)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
        var resolveDelay = _intervalParser.GetDelayResolver(interval);

        // warm up resolver
        resolveDelay(_timeProvider.Now.InUtc().LocalDateTime);

        _executor.Schedule(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                var time = _timeProvider.Now.InUtc().LocalDateTime;
                var delay = resolveDelay(time.CeilToSecond());
                var ms = Duration.FromMilliseconds(1000 - time.Millisecond);
                var total = (delay + ms).ToTimeSpan();

                await Task.Delay(total, cts.Token);

                if (!cts.IsCancellationRequested)
                    await handler();
            }
        });

        return Disposable.Create(() => cts.Cancel());
    }

    /// <summary>
    /// Asynchronously disposes the scheduler, cancelling all scheduled tasks and cleaning up resources
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous dispose operation</returns>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        this.Trace("cancel run");
        await _cts.CancelAsync();
        _cts.Dispose();

        this.Trace("await executor");
        await _executor.DisposeAsync();

        this.Trace("done");
    }
}
