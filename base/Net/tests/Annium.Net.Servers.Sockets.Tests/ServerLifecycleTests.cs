using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Servers.Sockets.Tests;

/// <summary>
/// Integration tests for IServer lifecycle: start, dispose, port release, and handler drain.
/// </summary>
public class ServerLifecycleTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerLifecycleTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xunit output helper.</param>
    public ServerLifecycleTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// DisposeAsync is idempotent: calling it a second time is a no-op and must not throw
    /// (the internal CancellationTokenSource is disposed once, guarded by an Interlocked flag).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        // arrange — no-op handler; we only exercise lifecycle, not data transfer
        var server = StartServer(
            (_, _, ct) =>
                Task.Delay(Timeout.Infinite, ct)
                    .ContinueWith(_ => { }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default)
        );

        // first dispose — must complete normally
        await server.DisposeAsync();

        // second dispose — must be a no-op (idempotent), not throw on the already-disposed CTS
        await server.DisposeAsync();
    }

    /// <summary>
    /// Disposing an idle server (no connected clients) must complete within the timeout.
    /// Pins that cancellation correctly breaks the AcceptSocketAsync loop.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_WhileIdle_CompletesWithinTimeout()
    {
        // arrange
        var server = StartServer(
            (_, _, ct) =>
                Task.Delay(Timeout.Infinite, ct)
                    .ContinueWith(_ => { }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default)
        );

        // act — dispose must not hang; 5 s is generous for a local loopback server
        await server.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        // assert — if WaitAsync did not throw TimeoutException, dispose completed in time
        // port must have been a valid ephemeral port (> 0) — sanity check start succeeded
        server.Port.IsGreater((ushort)0);
    }

    /// <summary>
    /// After disposing the first server, a second server can bind to the same port.
    /// Pins that DisposeAsync fully awaits shutdown so the OS releases the port before returning.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_ReleasesPort_SecondServerCanBind()
    {
        // arrange — start server on an ephemeral port, capture it
        var first = StartServer(
            (_, _, ct) =>
                Task.Delay(Timeout.Infinite, ct)
                    .ContinueWith(_ => { }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default)
        );
        var port = first.Port;
        port.IsGreater((ushort)0);

        // act — dispose first server; this must fully release the port
        await first.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        // assert — a second server can now bind to the same port
        var second = StartServerOnPort(
            port,
            (_, _, ct) =>
                Task.Delay(Timeout.Infinite, ct)
                    .ContinueWith(_ => { }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default)
        );
        second.IsNotNull();
        second!.Port.Is(port);

        // cleanup
        await second.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// When an in-flight handler is waiting on a gate, DisposeAsync must signal the handler's
    /// CancellationToken, wait for it to finish, and only then return — all within a bounded
    /// timeout. Pins the drain-on-dispose contract.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_WhileHandlerInFlight_AwaitsHandlerDrain()
    {
        // arrange
        var handlerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handlerCanceled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var server = StartServer(
            async (_, _, ct) =>
            {
                // signal that the handler has started
                handlerStarted.TrySetResult();

                // wait until the server's CTS is cancelled (i.e. DisposeAsync is called)
                try
                {
                    await Task.Delay(Timeout.Infinite, ct);
                }
                catch (OperationCanceledException)
                {
                    // expected: server is shutting down
                }

                // signal that the handler has observed cancellation and is returning
                handlerCanceled.TrySetResult();
            }
        );

        // connect a client to trigger the handler
        using var client = new TcpClient();
        await client
            .ConnectAsync(IPAddress.Loopback, server.Port, TestContext.Current.CancellationToken)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        // wait until the handler is running inside the server
        await handlerStarted.Task.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        // act — dispose the server; this must wait for the in-flight handler to finish
        await server.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        // assert — handler must have observed cancellation before (or simultaneous with) DisposeAsync completing.
        // If the executor does NOT drain, handlerCanceled.Task will not be set yet.
        handlerCanceled.Task.IsCompleted.IsTrue();
    }
}
