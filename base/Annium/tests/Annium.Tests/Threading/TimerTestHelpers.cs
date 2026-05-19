using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Threading.Tasks;

namespace Annium.Tests.Threading;

/// <summary>
/// Shared scaffolding for the AsyncTimer / SyncTimer test fixtures: a snapshot-safe state holder and a
/// bounded "ensure step ran to completion" assertion. The state class uses <see cref="ConcurrentQueue{T}"/>
/// so <c>ToArray</c> is safe against races with queued timer callbacks that may call <c>Push</c> after the
/// underlying timer was stopped via <c>Change(Infinite, Infinite)</c> but before its queued callbacks drained.
/// </summary>
internal static class TimerTestHelpers
{
    /// <summary>
    /// State holder used by the timer fixtures' step-counting assertions.
    /// </summary>
    public sealed class State
    {
        /// <summary>
        /// Gets the queue of integers recorded by <see cref="Push"/>.
        /// </summary>
        public ConcurrentQueue<int> Data { get; } = new();

        /// <summary>
        /// Adds the current count to the queue.
        /// </summary>
        public void Push() => Data.Enqueue(Data.Count);
    }

    /// <summary>
    /// Bounded wait until the timer's step is executed to completion (even number of items in the
    /// queue), then assert the queued sequence is 0..N-1. Replaces the previous unbounded
    /// <c>do { await Task.Delay(5); } while (count % 2 &gt; 0)</c> loop that could hang the test runner
    /// indefinitely if a timer regression caused the count to stop advancing.
    /// </summary>
    /// <param name="state">The state to validate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task EnsureValidAsync(State state)
    {
        await Wait.UntilAsync(() => state.Data.Count % 2 == 0, ms: 5000);

        var snapshot = state.Data.ToArray();
        var expectedData = Enumerable.Range(0, snapshot.Length).ToArray();
        snapshot.SequenceEqual(expectedData).IsTrue();
    }
}
