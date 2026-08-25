using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Workers.Tests;

/// <summary>
/// Start and stop are scheduled onto the manager's executor, which is concurrent. Nothing may let the two
/// halves of one worker's lifecycle run at the same time on the same instance: a stop that overlaps a
/// start tears down fields the start has not finished assigning.
/// </summary>
public class WorkerSequencingTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerSequencingTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public WorkerSequencingTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.Add<Overlap>().AsSelf().Singleton();
            container.AddWorkers<SlowWorkerData, SlowWorker>();
        });
    }

    /// <summary>
    /// Starting a key whose worker is still being stopped waits for that stop and starts a fresh worker,
    /// rather than reporting success over the one being torn down. The mirror of the case above: the
    /// manager owns the sequencing, so a caller that gets a start back has a worker that is running.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task StartAsync_WhileStopping_WaitsAndStartsAfresh()
    {
        // arrange
        var overlap = Get<Overlap>();
        var manager = Get<IWorkerManager<SlowWorkerData>>();
        var key = new SlowWorkerData("A");
        await manager.StartAsync(key);

        // act - a start arrives while the stop is still in flight
        var stop = manager.StopAsync(key);
        await manager.StartAsync(key);
#pragma warning disable VSTHRD003
        await stop;
#pragma warning restore VSTHRD003

        // if the second StartAsync reported a running worker, stopping again must actually stop one
        await manager.StopAsync(key);
        overlap.Stops.Is(2, "a start that reported success must leave a worker that can be stopped");
    }

    /// <summary>
    /// Stopping a worker whose start has not finished waits for that start instead of racing it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task StopAsync_WhileStillStarting_DoesNotOverlapTheStart()
    {
        // arrange
        var overlap = Get<Overlap>();
        var manager = Get<IWorkerManager<SlowWorkerData>>();
        var key = new SlowWorkerData("A");

        // act - stop is asked for while the start is still in its slow half
        var start = manager.StartAsync(key);
        await Task.Delay(TimeSpan.FromMilliseconds(30), TestContext.Current.CancellationToken);
        await manager.StopAsync(key);
#pragma warning disable VSTHRD003
        await start;
#pragma warning restore VSTHRD003

        // assert
        overlap.Count.Is(0, "stopping must not run while the worker is still starting");
        overlap.Stops.Is(1, "and the worker must still be stopped");
    }
}

/// <summary>
/// Records whether the two halves of a worker's lifecycle ever ran at the same time.
/// </summary>
public class Overlap
{
    /// <summary>
    /// Gets how many times a stop began while a start was in flight.
    /// </summary>
    public int Count => Volatile.Read(ref _count);

    /// <summary>
    /// Gets how many stops happened.
    /// </summary>
    public int Stops => Volatile.Read(ref _stops);

    /// <summary>
    /// Number of overlaps observed.
    /// </summary>
    private int _count;

    /// <summary>
    /// Number of stops observed.
    /// </summary>
    private int _stops;

    /// <summary>
    /// Number of starts currently in flight.
    /// </summary>
    private int _starting;

    /// <summary>
    /// Marks a start as begun.
    /// </summary>
    public void EnterStart() => Interlocked.Increment(ref _starting);

    /// <summary>
    /// Marks a start as finished.
    /// </summary>
    public void ExitStart() => Interlocked.Decrement(ref _starting);

    /// <summary>
    /// Records a stop, and whether it overlapped a start.
    /// </summary>
    public void EnterStop()
    {
        Interlocked.Increment(ref _stops);
        if (Volatile.Read(ref _starting) > 0)
            Interlocked.Increment(ref _count);
    }
}

/// <summary>
/// Test data model identifying a worker.
/// </summary>
/// <param name="Id">The unique identifier for the worker.</param>
public record SlowWorkerData(string Id);

/// <summary>
/// Worker whose start takes long enough for a stop to race it.
/// </summary>
public class SlowWorker : WorkerBase<SlowWorkerData>, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this worker.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Records the overlap, if any.
    /// </summary>
    private readonly Overlap _overlap;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlowWorker"/> class.
    /// </summary>
    /// <param name="overlap">The recorder to report into.</param>
    /// <param name="logger">Logger used for tracing.</param>
    public SlowWorker(Overlap overlap, ILogger logger)
    {
        Logger = logger;
        _overlap = overlap;
    }

    /// <summary>
    /// Takes its time, and deliberately does not observe cancellation - a worker whose start is already
    /// past the point of no return is exactly the case being pinned.
    /// </summary>
    /// <param name="ct">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous start operation.</returns>
    protected override async ValueTask StartAsync(CancellationToken ct)
    {
        _overlap.EnterStart();
        // xUnit1051: the point of this worker is a start that keeps going once cancellation is requested
#pragma warning disable xUnit1051
        await Task.Delay(TimeSpan.FromMilliseconds(200));
#pragma warning restore xUnit1051
        _overlap.ExitStart();
    }

    /// <summary>
    /// Records the stop.
    /// </summary>
    /// <returns>A task representing the asynchronous stop operation.</returns>
    protected override ValueTask StopAsync()
    {
        _overlap.EnterStop();

        return ValueTask.CompletedTask;
    }
}
