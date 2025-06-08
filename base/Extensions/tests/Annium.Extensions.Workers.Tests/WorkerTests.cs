using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Workers.Tests;

/// <summary>
/// Tests for worker management functionality including sequential, concurrent, and rapid control scenarios.
/// </summary>
public class WorkerTests : TestBase
{
    public WorkerTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.Add<SharedLog>().AsSelf().Singleton();
            container.AddWorkers<WorkerData, WorkerBase>();
        });
    }

    /// <summary>
    /// Tests sequential worker control by starting and stopping a single worker instance.
    /// Verifies that start and stop operations execute in the correct order.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Worker_Sequential_Control()
    {
        // arrange
        var log = Get<SharedLog>();
        var keyA = new WorkerData("A");
        var manager = Get<IWorkerManager<WorkerData>>();

        // act & assert
        await manager.StartAsync(keyA);
        log.GetLog(keyA).IsEqual(new[] { "start A" });
        await manager.StopAsync(keyA);
        log.GetLog(keyA).IsEqual(new[] { "start A", "done A" });
    }

    /// <summary>
    /// Tests concurrent worker control by starting and stopping multiple workers simultaneously.
    /// Verifies that multiple workers can be managed concurrently without interference.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Worker_Concurrent_Control()
    {
        // arrange
        var log = Get<SharedLog>();
        var keyA = new WorkerData("A");
        var keyB = new WorkerData("B");
        var manager = Get<IWorkerManager<WorkerData>>();

        // act
        await Task.WhenAll(
            Task.Run(() => manager.StartAsync(keyA), TestContext.Current.CancellationToken),
            Task.Run(() => manager.StartAsync(keyB), TestContext.Current.CancellationToken)
        );
        await Task.Delay(100, TestContext.Current.CancellationToken);
        await Task.WhenAll(
            Task.Run(() => manager.StopAsync(keyA), TestContext.Current.CancellationToken),
            Task.Run(() => manager.StopAsync(keyB), TestContext.Current.CancellationToken)
        );

        // assert
        await Expect.ToAsync(() =>
        {
            log.GetLog(keyA).IsEqual(new[] { "start A", "done A" });
            log.GetLog(keyB).IsEqual(new[] { "start B", "done B" });
        });
    }

    /// <summary>
    /// Tests rapid worker control by quickly starting and stopping workers multiple times.
    /// Verifies that the worker manager handles rapid start/stop operations gracefully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Worker_Fast_Control()
    {
        // arrange
        var log = Get<SharedLog>();
        var keyA = new WorkerData("A");
        var keyB = new WorkerData("B");
        var manager = Get<IWorkerManager<WorkerData>>();

        // act
        await Task.WhenAll(
            Task.Run(() => manager.StartAsync(keyA), TestContext.Current.CancellationToken),
            Task.Run(() => manager.StartAsync(keyB), TestContext.Current.CancellationToken),
            Task.Run(() => manager.StartAsync(keyA), TestContext.Current.CancellationToken),
            Task.Run(() => manager.StartAsync(keyB), TestContext.Current.CancellationToken)
        );
        await Task.WhenAll(
            Task.Run(() => manager.StopAsync(keyA), TestContext.Current.CancellationToken),
            Task.Run(() => manager.StopAsync(keyB), TestContext.Current.CancellationToken),
            Task.Run(() => manager.StopAsync(keyA), TestContext.Current.CancellationToken),
            Task.Run(() => manager.StopAsync(keyB), TestContext.Current.CancellationToken)
        );

        // assert
        await Expect.ToAsync(() =>
        {
            log.GetLog(keyA).IsEqual(new[] { "start A", "done A" });
            log.GetLog(keyB).IsEqual(new[] { "start B", "done B" });
        });
    }
}

/// <summary>
/// Test data model representing a worker instance with a unique identifier.
/// </summary>
/// <param name="Id">The unique identifier for the worker.</param>
file record WorkerData(string Id);

/// <summary>
/// Test implementation of a worker that tracks start and stop operations for testing purposes.
/// </summary>
file class WorkerBase : WorkerBase<WorkerData>, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this worker.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The shared log instance used to track worker operations for testing.
    /// </summary>
    private readonly SharedLog _log;

    public WorkerBase(SharedLog log, ILogger logger)
    {
        Logger = logger;
        _log = log;
    }

    /// <summary>
    /// Starts the worker by logging the start operation and adding a delay to simulate work.
    /// </summary>
    /// <param name="ct">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous start operation.</returns>
    protected override async ValueTask StartAsync(CancellationToken ct)
    {
        await Task.Delay(10, ct);

        this.Trace<string>("start {id}", Key.Id);
        _log.Track(Key, $"start {Key.Id}");
    }

    /// <summary>
    /// Stops the worker by logging the stop operation and adding a delay to simulate cleanup work.
    /// </summary>
    /// <returns>A task representing the asynchronous stop operation.</returns>
    protected override async ValueTask StopAsync()
    {
        await Task.Delay(10);

        _log.Track(Key, $"done {Key.Id}");
        this.Trace<string>("done {id}", Key.Id);
    }
}

/// <summary>
/// Shared logging utility for tracking worker operations during tests.
/// Provides thread-safe logging and retrieval of worker activity.
/// </summary>
file class SharedLog
{
    /// <summary>
    /// Thread-safe dictionary that maps worker data to their respective log entries.
    /// </summary>
    private readonly ConcurrentDictionary<WorkerData, TestLog<string>> _log = new();

    /// <summary>
    /// Tracks a message for the specified worker instance.
    /// </summary>
    /// <param name="workerData">The worker instance to track the message for.</param>
    /// <param name="message">The message to track.</param>
    public void Track(WorkerData workerData, string message)
    {
        _log.GetOrAdd(workerData, _ => new TestLog<string>()).Add(message);
    }

    /// <summary>
    /// Retrieves all logged messages for the specified worker instance.
    /// </summary>
    /// <param name="workerData">The worker instance to get logs for.</param>
    /// <returns>A read-only collection of logged messages, or an empty collection if no logs exist.</returns>
    public IReadOnlyCollection<string> GetLog(WorkerData workerData)
    {
        return _log.TryGetValue(workerData, out var log) ? log : Array.Empty<string>();
    }
}
