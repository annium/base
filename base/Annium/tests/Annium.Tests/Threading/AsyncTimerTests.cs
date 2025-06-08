using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Threading;
using Xunit;

namespace Annium.Tests.Threading;

/// <summary>
/// Contains unit tests for the AsyncTimer class.
/// </summary>
public class AsyncTimerTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the AsyncTimerTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public AsyncTimerTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that stateful timer works correctly with overlapping executions.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Stateful_Overlapping()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Async(
            state,
            static async state =>
            {
                state.Push();
                await Task.Delay(3);
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

    /// <summary>
    /// Verifies that stateful timer works correctly with concurrent starts.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Stateful_ConcurrentStart()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Async(
            state,
            static async state =>
            {
                state.Push();
                await Task.Delay(3);
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

    /// <summary>
    /// Verifies that stateless timer works correctly with overlapping executions.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Stateless_Overlapping()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Async(
            async () =>
            {
                state.Push();
                await Task.Delay(3);
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

    /// <summary>
    /// Verifies that stateless timer works correctly with concurrent starts.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Stateless_ConcurrentStart()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Async(
            async () =>
            {
                state.Push();
                await Task.Delay(3);
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

    /// <summary>
    /// Ensures that the state is valid by checking the sequence of numbers.
    /// </summary>
    /// <param name="state">The state to validate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
