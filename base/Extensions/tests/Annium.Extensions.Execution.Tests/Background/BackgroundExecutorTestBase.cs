using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit.Abstractions;

namespace Annium.Extensions.Execution.Tests.Background;

public abstract class BackgroundExecutorTestBase : TestBase
{
    private readonly IBackgroundExecutor _executor;

    protected BackgroundExecutorTestBase(Func<ILogger, IBackgroundExecutor> getExecutor, ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _executor = getExecutor(Get<ILogger>());
    }

    protected async Task<IReadOnlyList<int>> Works_Base(int size)
    {
        // run executor
        _executor.Start();

        // act
        // schedule batch of work
        var queue = new ConcurrentQueue<int>();
        foreach (var i in Enumerable.Range(0, size))
            _executor.Schedule(() =>
            {
                queue.Enqueue(i);
                Helper.SyncLongWork();
                queue.Enqueue(i + size);
            });

        // dispose to force processing finished
        await _executor.DisposeAsync();

        return queue.ToArray();
    }

    protected async Task Availability_Base()
    {
        // act
        // schedule batch of work
        Parallel.For(0, 4, _ => _executor.Schedule(Helper.SyncLongWork));

        // run executor
        _executor.Start();

        // assert
        _executor.IsAvailable.IsTrue();

        // init disposal
        var disposalTask = _executor.DisposeAsync();

        // assert
        _executor.IsAvailable.IsFalse();
        // throws, as not available already
        Wrap.It(() => _executor.Schedule(() => { })).Throws<InvalidOperationException>();

        // cleanup
        await disposalTask;
    }

    protected async Task HandlesFailure_Base()
    {
        // arrange
        var successes = 0;
        var failures = 0;

        // act
        // schedule batch of work
        Parallel.For(0, 10, i => _executor.Schedule(async () =>
        {
            await Task.Delay(10);
            if (i % 5 == 0)
            {
                Interlocked.Increment(ref failures);
                throw new Exception("Some failure");
            }

            Interlocked.Increment(ref successes);
        }));
        successes.Is(0);
        failures.Is(0);
        // run executor
        _executor.Start();
        // schedule another batch of work
        Parallel.For(0, 10, i => _executor.Schedule(async () =>
        {
            await Task.Delay(10);
            if (i % 5 == 0)
            {
                Interlocked.Increment(ref failures);
                throw new Exception("Some failure");
            }

            Interlocked.Increment(ref successes);
        }));

        // assert
        _executor.IsAvailable.IsTrue();
        // init disposal
        var disposalTask = _executor.DisposeAsync();
        _executor.IsAvailable.IsFalse();
        // throws, as not available already
        Wrap.It(() => _executor.Schedule(() => { })).Throws<InvalidOperationException>();
        await disposalTask;
        successes.Is(16);
        failures.Is(4);
    }

    protected async Task Schedule_SyncAction_Base()
    {
        // arrange
        var cts = new CancellationTokenSource();
        var success = false;

        // act
        _executor.Schedule(() => success = true);

        // run and dispose executor
        _executor.Start(cts.Token);
        await _executor.DisposeAsync();

        // assert
        success.IsTrue();
    }

    protected async Task Schedule_SyncCancellableAction_Base()
    {
        // arrange
        var cts = new CancellationTokenSource();
        var isCancelled = false;

        // act
        _executor.Schedule(ct => ct.Register(() => isCancelled = true));

        // run and dispose executor
        _executor.Start(cts.Token);
        await _executor.DisposeAsync();

        // assert
        isCancelled.IsTrue();
    }

    protected async Task Schedule_AsyncAction_Base()
    {
        // arrange
        var cts = new CancellationTokenSource();
        var success = false;

        // act
        _executor.Schedule(async () =>
        {
            await Task.Delay(50, CancellationToken.None);
            success = true;
        });

        // run and dispose executor
        _executor.Start(cts.Token);
        await _executor.DisposeAsync();

        // assert
        success.IsTrue();
    }

    protected async Task Schedule_AsyncCancellableAction_Base()
    {
        // arrange
        var cts = new CancellationTokenSource();
        var isCancelled = false;

        // act
        _executor.Schedule(async ct =>
        {
            await Task.Delay(50, CancellationToken.None);
            ct.Register(() => isCancelled = true);
        });

        // run and dispose executor
        _executor.Start(cts.Token);
        await _executor.DisposeAsync();

        // assert
        isCancelled.IsTrue();
    }
}