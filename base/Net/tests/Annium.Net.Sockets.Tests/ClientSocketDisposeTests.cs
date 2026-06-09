using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests;

/// <summary>
/// Tests that <see cref="ClientSocket.Dispose"/> is safe without a prior Connect and idempotent.
/// </summary>
public class ClientSocketDisposeTests
{
    /// <summary>
    /// Calling Dispose() without ever calling Connect() must not throw and leaves the socket
    /// disconnected. Verifies that the constructor-created CancellationTokenSource is disposed cleanly
    /// (if Dispose threw, the test would fail; the assertion confirms the disposed socket is disconnected).
    /// </summary>
    [Fact]
    public void Dispose_WithoutConnect_DoesNotThrowAndIsDisconnected()
    {
        var socket = new ClientSocket(ClientSocketOptions.Default, VoidLogger.Instance);

        socket.Dispose();

        socket.IsConnected.IsFalse();
    }

    /// <summary>
    /// Calling Dispose() a second time on the same socket must not throw and stays disconnected.
    /// </summary>
    [Fact]
    public void Dispose_CalledTwice_DoesNotThrowAndIsDisconnected()
    {
        var socket = new ClientSocket(ClientSocketOptions.Default, VoidLogger.Instance);

        socket.Dispose();
        socket.Dispose();

        socket.IsConnected.IsFalse();
    }
}
