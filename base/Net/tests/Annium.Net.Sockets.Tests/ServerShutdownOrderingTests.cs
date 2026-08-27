using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Annium.Logging;
using Xunit;

namespace Annium.Net.Sockets.Tests;

/// <summary>
/// Verifies the shutdown ordering of <c>Net.Servers.Sockets.Internal.Server</c>:
/// <c>_listener.Stop()</c> must run before the executor drains, so an in-flight handler
/// loop that respects cancellation can exit and the listener port can be released promptly.
/// </summary>
public class ServerShutdownOrderingTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerShutdownOrderingTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public ServerShutdownOrderingTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Spawns a server whose handler loops forever while respecting cancellation. Connects a
    /// client, waits for the handler to actually enter its loop, then disposes the server and
    /// asserts the dispose completes without hanging — the stop-then-drain ordering lets the
    /// in-flight handler observe cancellation and exit promptly.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_WithInfiniteLoopHandler_CompletesWithoutHanging()
    {
        this.Trace("start");

        var handlerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // arrange — server with a handler that signals once it enters its ct-respecting loop
        var server = RunServerBase(
            async (_, socket, ct) =>
            {
                handlerStarted.TrySetResult();

                try
                {
                    while (!ct.IsCancellationRequested)
                        await Task.Delay(100, ct);
                }
                catch (TaskCanceledException)
                {
                    // expected when ct fires
                }

                socket.Close();
            }
        );

        // connect a client so the handler starts running
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, server.Port, TestContext.Current.CancellationToken);

        // wait for the handler to actually enter its loop (replaces a fixed Task.Delay start race)
        await handlerStarted.Task.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        // act + assert — dispose must complete and NOT hang. A bounded wait fails fast on a
        // regression (wrong ordering → drain blocks on the in-flight handler) while tolerating CI
        // scheduling jitter, unlike a tight wall-clock comparison.
        var sw = Stopwatch.StartNew();
        await server.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
        sw.Stop();

        this.Trace<long>("dispose took {ms}ms", sw.ElapsedMilliseconds);

        this.Trace("done");
    }
}
