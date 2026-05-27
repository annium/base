using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for <see cref="AsyncLazy{T}"/> to verify lazy initialization behavior.
/// </summary>
public class AsyncLazyTest
{
    /// <summary>
    /// Verifies that the synchronous factory works as expected.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SyncFactory_Works()
    {
        // arrange
        var lazy = new AsyncLazy<int>(() => 10);

        // act
        var value = await lazy;

        // assert
        value.Is(10);
    }

    /// <summary>
    /// Verifies that the synchronous factory works correctly when accessed concurrently.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SyncFactory_Concurrent_Works()
    {
        // arrange
        var lazy = new AsyncLazy<object>(() => new object());

        // act
        var values = await Task.WhenAll(
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy)
        );

        // assert
        var subject = values[0];
        foreach (var value in values)
            value.Is(subject);
    }

    /// <summary>
    /// Verifies that the asynchronous factory works as expected.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncFactory_Works()
    {
        // arrange
        var lazy = new AsyncLazy<int>(async () =>
        {
            await Task.Delay(5);
            return 10;
        });

        // act
        var value = await lazy;

        // assert
        value.Is(10);
    }

    /// <summary>
    /// Verifies that the asynchronous factory works correctly when accessed concurrently.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncFactory_Concurrent_Works()
    {
        // arrange
        var lazy = new AsyncLazy<object>(async () =>
        {
            await Task.Delay(25);
            return new object();
        });

        // act
        var values = await Task.WhenAll(
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy)
        );

        // assert
        var subject = values[0];
        foreach (var value in values)
            value.Is(subject);
    }

    /// <summary>
    /// Verifies that <c>GetValueAsync</c> returns the lazily-produced value (T8 — replaces the
    /// removed <c>Value</c> sync trapdoor as the explicit accessor).
    /// </summary>
    [Fact]
    public async Task GetValueAsync_SyncFactory_ReturnsValue()
    {
        // arrange
        var lazy = new AsyncLazy<int>(() => 42);

        // act
        var value = await lazy.GetValueAsync(TestContext.Current.CancellationToken);

        // assert
        value.Is(42);
    }

    /// <summary>
    /// Verifies that <c>GetValueAsync</c> works for an async factory.
    /// </summary>
    [Fact]
    public async Task GetValueAsync_AsyncFactory_ReturnsValue()
    {
        // arrange
        var lazy = new AsyncLazy<int>(async () =>
        {
            await Task.Delay(5);
            return 99;
        });

        // act
        var value = await lazy.GetValueAsync(TestContext.Current.CancellationToken);

        // assert
        value.Is(99);
    }

    /// <summary>
    /// Verifies the <c>Value</c> property has been removed (T8 — sync trapdoor closed).
    /// </summary>
    [Fact]
    public void Value_PropertyDoesNotExist()
    {
        var prop = typeof(AsyncLazy<int>).GetProperty("Value");
        prop.Is(null);
    }

    /// <summary>
    /// Verifies that the second call to GetValueAsync after the underlying task has completed takes the
    /// fast path — the returned ValueTask completes synchronously without an async continuation. Closes
    /// the TG3 fast-path-coverage gap from review-2026.05.15.
    /// </summary>
    [Fact]
    public async Task GetValueAsync_AfterCompletion_UsesFastPath()
    {
        // arrange
        var lazy = new AsyncLazy<int>(() => 7);
        await lazy.GetValueAsync(TestContext.Current.CancellationToken);

        // act — second call should be synchronous via the IsCompletedSuccessfully fast path
        var task = lazy.GetValueAsync(TestContext.Current.CancellationToken);

        // assert — task is already completed before any continuation runs
        task.IsCompleted.IsTrue();
        (await task).Is(7);
    }

    /// <summary>
    /// Verifies that GetValueAsync on an AsyncLazy whose factory faulted unwraps the original
    /// exception on every subsequent access (T2 / B2) — i.e. callers see the real exception type
    /// rather than an AggregateException. Catches a regression where the fast path used IsCompleted
    /// (true for Faulted) and synchronously called task.Result, wrapping the exception.
    /// </summary>
    [Fact]
    public async Task GetValueAsync_FaultedFactory_PropagatesOriginalException()
    {
        // arrange — explicit Func<int> typing avoids ambiguity with the Func<Task<int>> overload
        Func<int> factory = () => throw new InvalidOperationException("boom");
        var lazy = new AsyncLazy<int>(factory);

        // act + assert — first call: original exception unwrapped
        var first = await Wrap.It(async () => await lazy.GetValueAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<InvalidOperationException>();
        first.Message.Is("boom");

        // act + assert — second call (fast path on faulted task): same unwrapped exception, NOT AggregateException
        var second = await Wrap.It(async () => await lazy.GetValueAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<InvalidOperationException>();
        second.Message.Is("boom");
    }

    /// <summary>
    /// Verifies that GetValueAsync respects a cancellation token threaded through WaitAsync on the
    /// slow path. Closes the cancellation-coverage gap from TG3.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetValueAsync_PreCancelledToken_ThrowsOperationCancelled()
    {
        // arrange — slow async factory so we hit the WaitAsync(ct) slow path
        var gate = new TaskCompletionSource<int>();
#pragma warning disable VSTHRD003 // gate is started immediately by AsyncLazy's Lazy<Task<int>>
        var lazy = new AsyncLazy<int>(() => gate.Task);
#pragma warning restore VSTHRD003
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // act + assert — WaitAsync should observe the pre-cancelled token and throw
        try
        {
            await Wrap.It(async () => await lazy.GetValueAsync(cts.Token)).ThrowsAsync<OperationCanceledException>();
        }
        finally
        {
            // cleanup — release the inner task so the test doesn't leak the gated factory even if the
            // assertion above throws
            gate.TrySetCanceled(TestContext.Current.CancellationToken);
        }
    }
}
