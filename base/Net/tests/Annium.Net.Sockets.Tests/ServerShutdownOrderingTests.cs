using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests;

/// <summary>
/// Verifies the shutdown ordering of <c>Net.Servers.Sockets.Internal.Server</c>:
/// <c>_listener.Stop()</c> must run before the executor drains, so an in-flight handler
/// loop that respects cancellation can exit and the listener port can be released promptly.
/// </summary>
public class ServerShutdownOrderingTests : TestBase
{
    public ServerShutdownOrderingTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Spawns a server whose handler loops forever while respecting cancellation.
    /// Connects a client, then disposes the server. Dispose must complete in under 1s
    /// with the new stop-then-drain ordering.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_WithInfiniteLoopHandler_CompletesInUnderOneSecond()
    {
        this.Trace("start");

        // arrange — server with a handler that loops while respecting ct
        var server = RunServerBase(
            async (_, socket, ct) =>
            {
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

        // give the handler a moment to start its loop
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // act — dispose and time it
        var sw = Stopwatch.StartNew();
        await server.DisposeAsync();
        sw.Stop();

        // assert — with stop-then-drain ordering, dispose completes well under 1s
        this.Trace<long>("dispose took {ms}ms", sw.ElapsedMilliseconds);
        sw.ElapsedMilliseconds.IsLess(1000);

        this.Trace("done");
    }
}
