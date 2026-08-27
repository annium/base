using System;
using System.Collections.Concurrent;
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
        await Bounded(start);
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
        await Bounded(stop);
        // VSTHRD003: `stop` is this test's own call
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await stop).ThrowsAsync<InvalidOperationException>();
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// Once the manager is disposed it refuses further work rather than quietly accepting a start that
    /// nothing will ever run — its executor is already gone by then.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task StartAsync_AfterDispose_Throws()
    {
        // arrange
        var manager = Get<IWorkerManager<FailingWorkerData>>();
        await ((IAsyncDisposable)manager).DisposeAsync();

        // act & assert - bounded: without the guard the call does not fail, it waits forever on a
        // worker the disposed executor will never run
        var start = manager.StartAsync(new FailingWorkerData("late", FailOn.Start));
        await Bounded(start);
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await start).ThrowsAsync<ObjectDisposedException>();
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// The same holds for stopping.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task StopAsync_AfterDispose_Throws()
    {
        // arrange
        var manager = Get<IWorkerManager<FailingWorkerData>>();
        await ((IAsyncDisposable)manager).DisposeAsync();

        // act & assert - bounded, for the same reason
        var stop = manager.StopAsync(new FailingWorkerData("late", FailOn.Stop));
        await Bounded(stop);
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await stop).ThrowsAsync<ObjectDisposedException>();
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// One worker failing to stop does not strand the others. Disposal is the last thing that knows a
    /// started worker exists, so a failure there that ends the loop leaves whatever those workers opened
    /// open for good - and which worker the loop reaches first is not something a caller controls.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DisposeAsync_OneWorkerFailsToStop_TheRestAreStillStopped()
    {
        // arrange - the failing one is started first and last, so neither iteration order spares it
        var manager = Get<IWorkerManager<FailingWorkerData>>();
        await manager.StartAsync(new FailingWorkerData("dispose-first", FailOn.None));
        await manager.StartAsync(new FailingWorkerData("dispose-bad", FailOn.Stop));
        await manager.StartAsync(new FailingWorkerData("dispose-last", FailOn.None));

        // act
        await ((IAsyncDisposable)manager).DisposeAsync();

        // assert
        FailingWorker
            .Stopped.ContainsKey("dispose-first")
            .IsTrue("a worker reached before the failing one must be stopped");
        FailingWorker
            .Stopped.ContainsKey("dispose-last")
            .IsTrue("a worker reached after the failing one must be stopped too");
    }

    /// <summary>
    /// Fails the test if the given call has not finished within five seconds, so a regression that turns a
    /// failure into an unbounded wait is reported as such instead of hanging the run.
    /// </summary>
    /// <param name="call">The call being bounded.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task Bounded(Task call)
    {
        // VSTHRD003: the call is the test's own, handed in precisely so its completion can be bounded
#pragma warning disable VSTHRD003
        var completed = await Task.WhenAny(
            call,
            Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)
        );
#pragma warning restore VSTHRD003
        (completed == call).IsTrue("the call must not wait indefinitely");
    }
}

/// <summary>
/// Which half of the worker lifecycle throws.
/// </summary>
file enum FailOn
{
    /// <summary>
    /// The worker does not throw at all.
    /// </summary>
    None,

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
    /// Gets the ids of the workers whose stop ran to completion, so a test can tell a worker that was
    /// stopped from one that was skipped.
    /// </summary>
    public static ConcurrentDictionary<string, byte> Stopped { get; } = new();

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

        Stopped[Key.Id] = 0;

        return ValueTask.CompletedTask;
    }
}
