using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit.Abstractions;

namespace Annium.Execution.Background.Tests;

public abstract class BackgroundExecutorTestBase : TestBase
{
    private readonly IExecutor _executor;

    protected BackgroundExecutorTestBase(Func<ILogger, IExecutor> getExecutor, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _executor = getExecutor(Get<ILogger>());
    }

    protected async Task<IReadOnlyList<int>> Works_Base(int size)
    {
        this.Trace("start");

        // run executor
        this.Trace("start executor");
        _executor.Start();

        // act
        // schedule batch of work
        this.Trace("schedule work");
        var queue = new ConcurrentQueue<int>();
        foreach (var i in Enumerable.Range(0, size))
            _executor.Schedule(() =>
            {
                queue.Enqueue(i);
                Helper.SyncLongWork();
                queue.Enqueue(i + size);
            });

        // dispose to force processing finished
        this.Trace("dispose executor");
        await _executor.DisposeAsync();

        this.Trace("done");

        return queue.ToArray();
    }

    protected async Task Availability_Base()
    {
        this.Trace("start");

        // act
        // schedule batch of work
        this.Trace("schedule work");
        Parallel.For(0, 4, _ => _executor.Schedule(Helper.SyncLongWork));

        // run executor
        this.Trace("start executor");
        _executor.Start();

        // assert
        this.Trace("ensure executor is available");
        _executor.IsAvailable.IsTrue();

        // init disposal
        this.Trace("start disposal");
        var disposalTask = _executor.DisposeAsync();

        // assert
        this.Trace("ensure executor is not available");
        _executor.IsAvailable.IsFalse();

        // throws, as not available already
        this.Trace("ensure executor schedule throws due to unavailability");
        Wrap.It(() => _executor.Schedule(() => { })).Throws<InvalidOperationException>();

        // cleanup
        this.Trace("await disposal");
        await disposalTask;

        this.Trace("done");
    }

    protected async Task HandlesFailure_Base()
    {
        this.Trace("start");

        // arrange
        var successes = 0;
        var failures = 0;

        // act
        // schedule batch of work
        this.Trace("schedule work");
        Parallel.For(
            0,
            10,
            i =>
                _executor.Schedule(async () =>
                {
                    await Task.Delay(10);
                    if (i % 5 == 0)
                    {
                        Interlocked.Increment(ref failures);
                        throw new Exception("Some failure");
                    }

                    Interlocked.Increment(ref successes);
                })
        );

        this.Trace("assert no events registered");
        successes.Is(0);
        failures.Is(0);

        // run executor
        this.Trace("start executor");
        _executor.Start();

        // schedule another batch of work
        this.Trace("schedule work");
        Parallel.For(
            0,
            10,
            i =>
                _executor.Schedule(async () =>
                {
                    await Task.Delay(10);
                    if (i % 5 == 0)
                    {
                        this.Trace("add failure");
                        Interlocked.Increment(ref failures);
                        throw new Exception("Some failure");
                    }

                    this.Trace("add success");
                    Interlocked.Increment(ref successes);
                })
        );

        // assert
        this.Trace("ensure executor is available");
        _executor.IsAvailable.IsTrue();

        // init disposal
        this.Trace("start disposal");
        var disposalTask = _executor.DisposeAsync();

        this.Trace("ensure executor is not available");
        _executor.IsAvailable.IsFalse();

        // throws, as not available already
        Wrap.It(() => _executor.Schedule(() => { })).Throws<InvalidOperationException>();

        this.Trace("await disposal");
        await disposalTask;

        this.Trace("assert events are registered");
        successes.Is(16);
        failures.Is(4);

        this.Trace("done");
    }

    protected async Task Schedule_SyncAction_Base()
    {
        this.Trace("start");

        // arrange
        var cts = new CancellationTokenSource();
        var success = false;

        // act
        this.Trace("schedule work");
        _executor.Schedule(() => success = true);

        // run and dispose executor
        this.Trace("start executor");
        _executor.Start(cts.Token);

        this.Trace("dispose executor");
        await _executor.DisposeAsync();

        // assert
        this.Trace("ensure work is complete");
        success.IsTrue();

        this.Trace("done");
    }

    protected async Task Schedule_SyncCancellableAction_Base()
    {
        this.Trace("start");

        // arrange
        var cts = new CancellationTokenSource();
        var isCancelled = false;

        // act
        this.Trace("schedule work");
        _executor.Schedule(ct => ct.Register(() => isCancelled = true));

        // run and dispose executor
        this.Trace("start executor");
        _executor.Start(cts.Token);

        this.Trace("dispose executor");
        await _executor.DisposeAsync();

        // assert
        this.Trace("ensure work is canceled");
        isCancelled.IsTrue();

        this.Trace("done");
    }

    protected async Task Schedule_AsyncAction_Base()
    {
        this.Trace("start");

        // arrange
        var cts = new CancellationTokenSource();
        var success = false;

        // act
        this.Trace("schedule work");
        _executor.Schedule(async () =>
        {
            await Task.Delay(50, CancellationToken.None);
            success = true;
        });

        // run and dispose executor
        this.Trace("start executor");
        _executor.Start(cts.Token);

        this.Trace("dispose executor");
        await _executor.DisposeAsync();

        // assert
        this.Trace("ensure work is complete");
        success.IsTrue();

        this.Trace("done");
    }

    protected async Task Schedule_AsyncCancellableAction_Base()
    {
        this.Trace("start");

        // arrange
        var cts = new CancellationTokenSource();
        var isCancelled = false;

        // act
        this.Trace("schedule work");
        _executor.Schedule(async ct =>
        {
            await Task.Delay(50, CancellationToken.None);
            ct.Register(() => isCancelled = true);
        });

        // run and dispose executor
        this.Trace("start executor");
        _executor.Start(cts.Token);

        this.Trace("dispose executor");
        await _executor.DisposeAsync();

        // assert
        this.Trace("ensure work is canceled");
        isCancelled.IsTrue();

        this.Trace("done");
    }
}
