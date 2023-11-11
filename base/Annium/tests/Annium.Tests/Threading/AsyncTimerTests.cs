using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Threading;
using Xunit;

namespace Annium.Tests.Threading;

public class AsyncTimerTests
{
    [Fact]
    public async Task Stateful_Overlapping()
    {
        // arrange
        var state = new State();
        using var timer = AsyncTimer.Create(
            state,
            static async state =>
            {
                state.Push();
                await Task.Delay(3);
                state.Push();
            },
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(1)
        );

        // act
        await Task.Delay(50);
        timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        // assert
        await EnsureValid(state);
    }

    [Fact]
    public async Task Stateful_ConcurrentStart()
    {
        // arrange
        var state = new State();
        using var timer = AsyncTimer.Create(
            state,
            static async state =>
            {
                state.Push();
                await Task.Delay(3);
                state.Push();
            },
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(1)
        );
        timer.Change(TimeSpan.Zero, TimeSpan.FromTicks(500));

        // act
        await Task.Delay(50);
        timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        // assert
        await EnsureValid(state);
    }

    [Fact]
    public async Task Stateless_Overlapping()
    {
        // arrange
        var state = new State();
        using var timer = AsyncTimer.Create(
            async () =>
            {
                state.Push();
                await Task.Delay(3);
                state.Push();
            },
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(1)
        );

        // act
        await Task.Delay(50);
        timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        // assert
        await EnsureValid(state);
    }

    [Fact]
    public async Task Stateless_ConcurrentStart()
    {
        // arrange
        var state = new State();
        using AsyncTimer timer = AsyncTimer.Create(
            async () =>
            {
                state.Push();
                await Task.Delay(3);
                state.Push();
            },
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(1)
        );
        timer.Change(TimeSpan.Zero, TimeSpan.FromTicks(500));

        // act
        await Task.Delay(50);
        timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        // assert
        await EnsureValid(state);
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
