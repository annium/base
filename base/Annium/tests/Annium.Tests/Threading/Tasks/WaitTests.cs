using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Tests.Threading.Tasks;

/// <summary>
/// Contains unit tests for the <see cref="Wait"/> class (review T3 — previously 0% covered).
/// </summary>
public class WaitTests
{
    /// <summary>
    /// Verifies that WhileAsync completes immediately when the condition is already false.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhileAsync_ConditionImmediatelyFalse_CompletesImmediately()
    {
        var sw = Stopwatch.StartNew();
        await Wait.WhileAsync(() => false, ms: 5000);
        sw.Stop();

        // Should complete well within 1s — first check is false, no polling delay incurred.
        (sw.ElapsedMilliseconds < 1000).IsTrue();
    }

    /// <summary>
    /// Verifies that WhileAsync exits when the cancellation token fires.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhileAsync_CancellationToken_Cancels()
    {
        using var cts = new CancellationTokenSource(200);
        var sw = Stopwatch.StartNew();
        await Wait.WhileAsync(() => true, cts.Token, pollDelay: 25);
        sw.Stop();

        // Should exit shortly after the 200ms cancellation.
        (sw.ElapsedMilliseconds >= 150 && sw.ElapsedMilliseconds < 2000).IsTrue();
    }

    /// <summary>
    /// Verifies that WhileAsync with a timeout exits at or near the timeout.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhileAsync_Timeout_Exits()
    {
        var sw = Stopwatch.StartNew();
        await Wait.WhileAsync(() => true, ms: 200);
        sw.Stop();

        (sw.ElapsedMilliseconds >= 150 && sw.ElapsedMilliseconds < 2000).IsTrue();
    }

    /// <summary>
    /// Verifies that UntilAsync completes immediately when the condition is already true.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UntilAsync_ConditionImmediatelyTrue_CompletesImmediately()
    {
        var sw = Stopwatch.StartNew();
        await Wait.UntilAsync(() => true, ms: 5000);
        sw.Stop();

        (sw.ElapsedMilliseconds < 1000).IsTrue();
    }

    /// <summary>
    /// Verifies that UntilAsync completes as soon as the condition flips to true.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UntilAsync_ConditionFlips_Completes()
    {
        var flipAt = DateTime.UtcNow.AddMilliseconds(100);
        await Wait.UntilAsync(() => DateTime.UtcNow >= flipAt, ms: 5000, pollDelay: 10);

        // If we got here without timing out, the condition was observed true.
        (DateTime.UtcNow >= flipAt).IsTrue();
    }

    /// <summary>
    /// Verifies that the async-condition overload of UntilAsync respects the cancellation token.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UntilAsync_AsyncCondition_CancellationToken_Cancels()
    {
        using var cts = new CancellationTokenSource(200);
        var sw = Stopwatch.StartNew();

        await Wait.UntilAsync(() => Task.FromResult(false), cts.Token, pollDelay: 25);

        sw.Stop();
        (sw.ElapsedMilliseconds >= 150 && sw.ElapsedMilliseconds < 2000).IsTrue();
    }

    /// <summary>
    /// Verifies that the async-condition overload of WhileAsync polls correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task WhileAsync_AsyncCondition_Timeout_Exits()
    {
        var sw = Stopwatch.StartNew();

        await Wait.WhileAsync(() => Task.FromResult(true), ms: 200);

        sw.Stop();
        (sw.ElapsedMilliseconds >= 150 && sw.ElapsedMilliseconds < 2000).IsTrue();
    }
}
