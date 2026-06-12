using System;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Tests.Threading.Tasks;

/// <summary>
/// Contains unit tests for the <see cref="AwaitableExtensions"/> class (review T4 — previously 0% covered).
/// </summary>
public class AwaitableExtensionsTests
{
    /// <summary>
    /// Verifies that <c>Task.Await()</c> blocks until completion of a non-result task.
    /// </summary>
    [Fact]
    public void Await_Task_BlocksUntilCompletion()
    {
        var ct = TestContext.Current.CancellationToken;
        var completed = false;
        Task.Run(
                async () =>
                {
                    await Task.Delay(50, ct);
                    completed = true;
                },
                ct
            )
            .Await();

        completed.IsTrue();
    }

    /// <summary>
    /// Verifies that <c>Task&lt;T&gt;.Await()</c> returns the task's result.
    /// </summary>
    [Fact]
    public void Await_TaskT_ReturnsResult()
    {
        var result = Task.FromResult(42).Await();

        result.Is(42);
    }

    /// <summary>
    /// Verifies that <c>ValueTask.Await()</c> blocks until completion of a non-result value-task.
    /// </summary>
    [Fact]
    public void Await_ValueTask_BlocksUntilCompletion()
    {
        var ct = TestContext.Current.CancellationToken;
        var completed = false;
        new ValueTask(
            Task.Run(
                async () =>
                {
                    await Task.Delay(50, ct);
                    completed = true;
                },
                ct
            )
        ).Await();

        completed.IsTrue();
    }

    /// <summary>
    /// Verifies that <c>ValueTask&lt;T&gt;.Await()</c> returns the value-task's result.
    /// </summary>
    [Fact]
    public void Await_ValueTaskT_ReturnsResult()
    {
        var result = new ValueTask<string>("hello").Await();

        result.Is("hello");
    }

    /// <summary>
    /// Verifies that a faulted Task rethrows its original exception unwrapped through Await().
    /// </summary>
    [Fact]
    public void Await_FaultedTask_RethrowsOriginalException()
    {
        var task = Task.FromException(new InvalidOperationException("boom"));

        var ex = Wrap.It(() => task.Await()).Throws<InvalidOperationException>();
        ex.Message.Is("boom");
    }

    /// <summary>
    /// Verifies that a faulted ValueTask rethrows its original exception unwrapped through Await().
    /// </summary>
    [Fact]
    public void Await_FaultedValueTask_RethrowsOriginalException()
    {
        var task = new ValueTask(Task.FromException(new ArgumentException("bad")));

        var ex = Wrap.It(() => task.Await()).Throws<ArgumentException>();
        ex.Message.Is("bad");
    }
}
