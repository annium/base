using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Extensions.Workers.Tests;

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

    [Fact]
    public async Task Worker_Concurrent_Control()
    {
        // arrange
        var log = Get<SharedLog>();
        var keyA = new WorkerData("A");
        var keyB = new WorkerData("B");
        var manager = Get<IWorkerManager<WorkerData>>();

        // act
        await Task.WhenAll(Task.Run(() => manager.StartAsync(keyA)), Task.Run(() => manager.StartAsync(keyB)));
        await Task.Delay(100);
        await Task.WhenAll(Task.Run(() => manager.StopAsync(keyA)), Task.Run(() => manager.StopAsync(keyB)));

        // assert
        await Expect.ToAsync(() =>
        {
            log.GetLog(keyA).IsEqual(new[] { "start A", "done A" });
            log.GetLog(keyB).IsEqual(new[] { "start B", "done B" });
        });
    }

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
            Task.Run(() => manager.StartAsync(keyA)),
            Task.Run(() => manager.StartAsync(keyB)),
            Task.Run(() => manager.StartAsync(keyA)),
            Task.Run(() => manager.StartAsync(keyB))
        );
        await Task.WhenAll(
            Task.Run(() => manager.StopAsync(keyA)),
            Task.Run(() => manager.StopAsync(keyB)),
            Task.Run(() => manager.StopAsync(keyA)),
            Task.Run(() => manager.StopAsync(keyB))
        );

        // assert
        await Expect.ToAsync(() =>
        {
            log.GetLog(keyA).IsEqual(new[] { "start A", "done A" });
            log.GetLog(keyB).IsEqual(new[] { "start B", "done B" });
        });
    }
}

file record WorkerData(string Id);

file class WorkerBase : WorkerBase<WorkerData>, ILogSubject
{
    public ILogger Logger { get; }
    private readonly SharedLog _log;

    public WorkerBase(SharedLog log, ILogger logger)
    {
        Logger = logger;
        _log = log;
    }

    protected override async ValueTask StartAsync(CancellationToken ct)
    {
        await Task.Delay(10, ct);

        this.Trace<string>("start {id}", Key.Id);
        _log.Track(Key, $"start {Key.Id}");
    }

    protected override async ValueTask StopAsync()
    {
        await Task.Delay(10);

        _log.Track(Key, $"done {Key.Id}");
        this.Trace<string>("done {id}", Key.Id);
    }
}

file class SharedLog
{
    private readonly ConcurrentDictionary<WorkerData, TestLog<string>> _log = new();

    public void Track(WorkerData workerData, string message)
    {
        _log.GetOrAdd(workerData, _ => new TestLog<string>()).Add(message);
    }

    public IReadOnlyCollection<string> GetLog(WorkerData workerData)
    {
        return _log.TryGetValue(workerData, out var log) ? log : Array.Empty<string>();
    }
}
