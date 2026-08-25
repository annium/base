using System;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests;

/// <summary>
/// Bounds an await, so a regression that stops a signal ever arriving is reported as a failed test rather
/// than a run that hangs until the CI job times out with nothing to point at.
/// </summary>
public static class Bounded
{
    /// <summary>
    /// The time any of these tests may wait for a signal. Generous: the point is to fail rather than hang,
    /// not to measure anything.
    /// </summary>
    private static readonly TimeSpan _limit = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Fails the test if the given task has not finished within the limit.
    /// </summary>
    /// <param name="task">The task being bounded.</param>
    /// <param name="because">What the wait was for, used in the failure message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task AwaitAsync(Task task, string because = "the awaited signal must arrive")
    {
        // VSTHRD003: bounding a task started elsewhere is the whole point of this helper
#pragma warning disable VSTHRD003
        var completed = await Task.WhenAny(task, Task.Delay(_limit, TestContext.Current.CancellationToken));
#pragma warning restore VSTHRD003
        (completed == task).IsTrue(because);

        // deliberately not re-awaited: several callers bound a task they expect to have faulted, and assert
        // on the failure themselves afterwards
    }
}
