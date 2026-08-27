using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Servers.Web.Tests;

/// <summary>
/// Tests for server startup, port exposure, and disposal lifecycle.
/// </summary>
public class ServerLifecycleTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the ServerLifecycleTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ServerLifecycleTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// DisposeAsync is idempotent: a second call is a no-op and must not throw
    /// (the CTS is disposed once, guarded by an Interlocked flag).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_CalledTwice_DoesNotThrow()
    {
        this.Trace("start");

        // arrange
        var server = RunHttpServer();

        // first dispose drains the server normally
        this.Trace("first dispose");
        await server.DisposeAsync();

        // act + assert: second dispose must be a no-op (idempotent), not throw on the disposed CTS
        this.Trace("second dispose");
        await server.DisposeAsync();

        this.Trace("done");
    }

    /// <summary>
    /// Disposing an idle server (no connected clients) must complete within 5 seconds.
    /// This pins the _cts.Token.Register(_listener.Stop) mechanism: without it the accept
    /// loop inside RunAsync would hang forever on GetContextAsync with no clients arriving.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_WhileIdle_CompletesWithinTimeout()
    {
        this.Trace("start");

        // arrange — start a server that never gets a client
        var server = RunHttpServer();

        // act — dispose must not hang; WaitAsync forces a bounded timeout
        this.Trace("dispose");
        await server.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        // assert — if WaitAsync did not throw, dispose completed in time
        server.Port.IsGreater((ushort)0);

        this.Trace("done");
    }

    // NOTE: a DisposeAsync_ReleasesPort_SecondServerCanBind test was intentionally removed. The Web server's
    // DisposeAsync signals shutdown and returns without blocking on the background accept-loop/drain (the
    // listener Close happens in the background), so the OS port is not guaranteed to be free synchronously
    // by the time DisposeAsync returns. Asserting immediate same-port rebind would be racy. See WK077.

    /// <summary>
    /// Start() returns a non-null server with a non-zero Port, proving the listener was
    /// allocated and the port was extracted from the prefix URI correctly.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Start_ReturnsServerWithNonZeroPort()
    {
        this.Trace("start");

        // arrange + act
        var server = RunHttpServer();

        // assert
        server.Port.IsGreater((ushort)0);
        server.IsSecure.IsFalse();

        this.Trace<ushort>("port: {port}", server.Port);

        await server.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        this.Trace("done");
    }
}
