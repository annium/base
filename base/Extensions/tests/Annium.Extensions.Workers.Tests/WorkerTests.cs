using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Testing;
using Annium.Testing.Assertions;
using Annium.Testing.Lib;
using Annium.Threading;
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
            container.AddWorkers<WorkerData, Worker>();
        });
    }

    [Fact]
    public async Task Worker_Normal_Control()
    {
        // arrange
        var log = Get<SharedLog>();
        var keyA = new WorkerData("A");
        var keyB = new WorkerData("B");
        var manager = Get<IWorkerManager<WorkerData>>();

        // act
        manager.Start(keyA);
        manager.Start(keyB);
        await Task.Delay(100);
        manager.Stop(keyA);
        manager.Stop(keyB);

        // assert
        await Expect.To(() =>
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
        manager.Start(keyA);
        manager.Start(keyB);
        manager.Start(keyA);
        manager.Start(keyB);
        manager.Stop(keyA);
        manager.Stop(keyB);
        manager.Stop(keyA);
        manager.Stop(keyB);

        // assert
        await Expect.To(() =>
        {
            log.GetLog(keyA).IsEqual(new[] { "start A", "done A" });
            log.GetLog(keyB).IsEqual(new[] { "start B", "done B" });
        });
    }
}

file record WorkerData(string Id);

file class Worker : IWorker<WorkerData>, ILogSubject
{
    public ILogger Logger { get; }
    private readonly SharedLog _log;

    public Worker(
        SharedLog log,
        ILogger logger
    )
    {
        Logger = logger;
        _log = log;
    }

    public async ValueTask RunAsync(WorkerData key, CancellationToken ct)
    {
        this.Trace<string>("start {id}", key.Id);
        _log.Track(key, $"start {key.Id}");
        await ct;
        _log.Track(key, $"done {key.Id}");
        this.Trace<string>("done {id}", key.Id);
    }
}

file class SharedLog
{
    private readonly ConcurrentDictionary<WorkerData, ConcurrentQueue<string>> _log = new();

    public void Track(WorkerData workerData, string message)
    {
        _log.GetOrAdd(workerData, _ => new ConcurrentQueue<string>()).Enqueue(message);
    }

    public IReadOnlyCollection<string> GetLog(WorkerData workerData)
    {
        return _log.TryGetValue(workerData, out var log) ? log : Array.Empty<string>();
    }
}