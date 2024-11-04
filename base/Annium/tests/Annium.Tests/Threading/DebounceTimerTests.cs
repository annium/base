using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Tests.Threading;

public class DebounceTimerTests : TestBase
{
    public DebounceTimerTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public async Task Stateful()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Debounce(
            state,
            s =>
            {
                this.Trace("start");
                s.Push();
                this.Trace("done");

                return ValueTask.CompletedTask;
            },
            20,
            Logger
        );

        // act
        this.Trace("schedule");
        for (var i = 0; i < 3; i++)
        {
            this.Trace("bulk start");
            for (var j = 0; j < 3; j++)
            {
                this.Trace("chunk start");
                Parallel.ForEach(Enumerable.Range(0, 5), _ => timer.Request());
                this.Trace("chunk delay");
                await Task.Delay(1);
            }
            this.Trace("bulk delay");
            await Task.Delay(50);
        }

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state, 3, 5);

        this.Trace("done");
    }

    [Fact]
    public async Task Stateless()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Debounce(
            () =>
            {
                this.Trace("start");
                state.Push();
                this.Trace("done");

                return ValueTask.CompletedTask;
            },
            20,
            Logger
        );

        // act
        this.Trace("schedule");
        for (var i = 0; i < 3; i++)
        {
            this.Trace("bulk start");
            for (var j = 0; j < 3; j++)
            {
                this.Trace("chunk start");
                Parallel.ForEach(Enumerable.Range(0, 5), _ => timer.Request());
                this.Trace("chunk delay");
                await Task.Delay(1);
            }
            this.Trace("bulk delay");
            await Task.Delay(50);
        }

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state, 3, 5);

        this.Trace("done");
    }

    private async Task EnsureValid(State state, int min, int max)
    {
        this.Trace("await for {min}-{max} entries in state", min, max);
        await Expect.ToAsync(
            () =>
            {
                state.Data.Count.IsGreaterOrEqual(min);
                state.Data.Count.IsLessOrEqual(max);
            },
            1000
        );

        this.Trace("verify state integrity");
        var expectedData = Enumerable.Range(0, state.Data.Count).ToArray();
        state.Data.SequenceEqual(expectedData).IsTrue();

        this.Trace("done");
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
