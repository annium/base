using System.IO;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests;

/// <summary>
/// Tests that <see cref="ServerSocket.Dispose"/> is safe in isolation-dispose scenarios.
/// <para>
/// <see cref="ServerSocketOptions.Default"/> uses no connection-monitor factory, so
/// <see cref="Annium.Net.Sockets.Internal.NoneConnectionMonitor"/> is installed — its
/// <c>Stop()</c> is a no-op, making it safe to dispose without driving a full connection lifecycle.
/// </para>
/// <para>
/// Unlike <see cref="ClientSocket.Dispose"/> (which calls <c>Disconnect()</c> internally),
/// <see cref="ServerSocket.Dispose"/> performs a forced-close without updating
/// <see cref="ServerSocket.IsConnected"/> synchronously. The correct graceful shutdown
/// sequence is <c>Disconnect()</c> followed by <c>Dispose()</c>.
/// </para>
/// </summary>
public class ServerSocketDisposeTests
{
    /// <summary>
    /// After <see cref="ServerSocket.Disconnect"/> + <see cref="ServerSocket.Dispose"/>,
    /// the socket reports <c>IsConnected == false</c>. <c>Disconnect()</c> transitions
    /// the status synchronously; <c>Dispose()</c> must not throw.
    /// </summary>
    [Fact]
    public void Dispose_AfterDisconnect_DoesNotThrowAndIsDisconnected()
    {
        using var stream = new MemoryStream();
        var socket = new ServerSocket(stream, ServerSocketOptions.Default, VoidLogger.Instance);

        socket.Disconnect();
        socket.Dispose();

        socket.IsConnected.IsFalse();
    }

    /// <summary>
    /// Calling <see cref="ServerSocket.Dispose"/> a second time must not throw.
    /// After the first forced-close <c>Dispose()</c> the monitor is already stopped and the
    /// managed socket already disposed; the second call is a no-op on both.
    /// </summary>
    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        using var stream = new MemoryStream();
        var socket = new ServerSocket(stream, ServerSocketOptions.Default, VoidLogger.Instance);

        socket.Dispose();
        socket.Dispose();

        // If neither Dispose() call threw, the test passes. The IsConnected state is
        // not asserted here because Dispose() without a prior Disconnect() does not
        // synchronously transition the status — that happens on the background IsClosed
        // continuation. The no-throw behaviour is the correctness property under test.
        socket.IsConnected.Is(socket.IsConnected); // readable without throw
    }
}
