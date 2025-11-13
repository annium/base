using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Threading;
using Xunit;

namespace Annium.Tests.Threading;

/// <summary>
/// Contains unit tests for the DebounceTimer class.
/// </summary>
public class DebounceTimerTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the DebounceTimerTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public DebounceTimerTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that stateful debounce timer works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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
                await Task.Delay(1, TestContext.Current.CancellationToken);
            }
            this.Trace("bulk delay");
            await Task.Delay(50, TestContext.Current.CancellationToken);
        }

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state, 3, 5);

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that stateless debounce timer works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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
                await Task.Delay(1, TestContext.Current.CancellationToken);
            }
            this.Trace("bulk delay");
            await Task.Delay(50, TestContext.Current.CancellationToken);
        }

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state, 3, 5);

        this.Trace("done");
    }

    /// <summary>
    /// Ensures that the state is valid by checking the sequence of numbers and count.
    /// </summary>
    /// <param name="state">The state to validate.</param>
    /// <param name="min">The minimum expected count.</param>
    /// <param name="max">The maximum expected count.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task EnsureValid(State state, int min, int max)
    {
        this.Trace("await for {min}-{max} entries in state", min, max);
        await Expect.ToAsync(() =>
        {
            state.Data.Count.IsGreaterOrEqual(min);
            state.Data.Count.IsLessOrEqual(max);
        });

        this.Trace("verify state integrity");
        var expectedData = Enumerable.Range(0, state.Data.Count).ToArray();
        state.Data.SequenceEqual(expectedData).IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// A class that maintains a queue of integers for testing.
    /// </summary>
    private class State
    {
        /// <summary>
        /// Gets the queue of integers.
        /// </summary>
        public Queue<int> Data { get; } = new();

        /// <summary>
        /// Adds the current count to the queue.
        /// </summary>
        public void Push()
        {
            Data.Enqueue(Data.Count);
        }
    }
}
