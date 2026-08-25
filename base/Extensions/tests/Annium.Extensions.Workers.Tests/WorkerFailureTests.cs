using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Workers.Tests;

/// <summary>
/// Tests for what a failing worker does to the caller that started or stopped it. Start and stop are awaited
/// through a completion signal set by background work, so a worker that throws must fail that signal —
/// otherwise the caller waits for a worker that will never report either way.
/// </summary>
public class WorkerFailureTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerFailureTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public WorkerFailureTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddWorkers<FailingWorkerData, FailingWorker>());
    }

    /// <summary>
    /// A worker whose start throws surfaces that failure to the caller instead of hanging it.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task StartAsync_WorkerThrows_Faults()
    {
        // arrange
        var manager = Get<IWorkerManager<FailingWorkerData>>();
        var key = new FailingWorkerData("start-fails", FailOn.Start);

        // act & assert - bounded, because the failure this pins is an unbounded wait
        var start = manager.StartAsync(key);
        var completed = await Task.WhenAny(
            start,
            Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)
        );
        (completed == start).IsTrue("StartAsync must not wait for a worker that already failed");
        // VSTHRD003: `start` is this test's own call, awaited to observe how the failure surfaces
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await start).ThrowsAsync<InvalidOperationException>();
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// A worker whose stop throws likewise surfaces the failure rather than leaving the caller waiting.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task StopAsync_WorkerThrows_Faults()
    {
        // arrange
        var manager = Get<IWorkerManager<FailingWorkerData>>();
        var key = new FailingWorkerData("stop-fails", FailOn.Stop);
        await manager.StartAsync(key);

        // act & assert
        var stop = manager.StopAsync(key);
        var completed = await Task.WhenAny(
            stop,
            Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)
        );
        (completed == stop).IsTrue("StopAsync must not wait for a worker that already failed");
        // VSTHRD003: `stop` is this test's own call
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await stop).ThrowsAsync<InvalidOperationException>();
#pragma warning restore VSTHRD003
    }
}

/// <summary>
/// Which half of the worker lifecycle throws.
/// </summary>
file enum FailOn
{
    /// <summary>
    /// The worker throws while starting.
    /// </summary>
    Start,

    /// <summary>
    /// The worker throws while stopping.
    /// </summary>
    Stop,
}

/// <summary>
/// Test data model identifying a worker and the lifecycle step it fails on.
/// </summary>
/// <param name="Id">The unique identifier for the worker.</param>
/// <param name="Fails">The lifecycle step that throws.</param>
file record FailingWorkerData(string Id, FailOn Fails);

/// <summary>
/// Worker that throws on whichever lifecycle step its key names.
/// </summary>
file class FailingWorker : WorkerBase<FailingWorkerData>, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this worker.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FailingWorker"/> class.
    /// </summary>
    /// <param name="logger">Logger used for tracing.</param>
    public FailingWorker(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Throws if the key says the start step should fail.
    /// </summary>
    /// <param name="ct">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous start operation.</returns>
    protected override ValueTask StartAsync(CancellationToken ct)
    {
        if (Key.Fails == FailOn.Start)
            throw new InvalidOperationException($"worker {Key.Id} failed to start");

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Throws if the key says the stop step should fail.
    /// </summary>
    /// <returns>A task representing the asynchronous stop operation.</returns>
    protected override ValueTask StopAsync()
    {
        if (Key.Fails == FailOn.Stop)
            throw new InvalidOperationException($"worker {Key.Id} failed to stop");

        return ValueTask.CompletedTask;
    }
}
