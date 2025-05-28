using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Threading;
using Xunit;

namespace Annium.Tests.Threading;

public class SyncTimerTests : TestBase
{
    public SyncTimerTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public async Task Stateful_Overlapping()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Sync(
            state,
            static state =>
            {
                state.Push();
                Thread.Sleep(3);
                state.Push();
            },
            0,
            1,
            Logger
        );

        // act
        await Task.Delay(50, TestContext.Current.CancellationToken);
        timer.Change(Timeout.Infinite, Timeout.Infinite);

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state);

        this.Trace("done");
    }

    [Fact]
    public async Task Stateful_ConcurrentStart()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Sync(
            state,
            static state =>
            {
                state.Push();
                Thread.Sleep(3);
                state.Push();
            },
            0,
            2,
            Logger
        );
        timer.Change(0, 1);

        // act
        await Task.Delay(50, TestContext.Current.CancellationToken);
        timer.Change(Timeout.Infinite, Timeout.Infinite);

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state);

        this.Trace("done");
    }

    [Fact]
    public async Task Stateless_Overlapping()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Sync(
            () =>
            {
                state.Push();
                Thread.Sleep(3);
                state.Push();
            },
            0,
            1,
            Logger
        );

        // act
        await Task.Delay(50, TestContext.Current.CancellationToken);
        timer.Change(Timeout.Infinite, Timeout.Infinite);

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state);

        this.Trace("done");
    }

    [Fact]
    public async Task Stateless_ConcurrentStart()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Sync(
            () =>
            {
                state.Push();
                Thread.Sleep(3);
                state.Push();
            },
            0,
            2,
            Logger
        );
        timer.Change(0, 1);

        // act
        await Task.Delay(50, TestContext.Current.CancellationToken);
        timer.Change(Timeout.Infinite, Timeout.Infinite);

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state);

        this.Trace("done");
    }

    private async Task EnsureValid(State state)
    {
        // await until timers complete (step is executed to end)
        do
        {
            await Task.Delay(5);
        } while (state.Data.Count % 2 > 0);

        var expectedData = Enumerable.Range(0, state.Data.Count).ToArray();
        state.Data.SequenceEqual(expectedData).IsTrue();
    }

    private class State
    {
        public Queue<int> Data { get; } = new();

        public void Push()
        {
            Data.Enqueue(Data.Count);
        }
    }
}
