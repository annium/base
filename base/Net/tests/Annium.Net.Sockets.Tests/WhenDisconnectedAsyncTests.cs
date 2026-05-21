using System;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests;

/// <summary>
/// Regression tests for <see cref="ClientSocketExtensions.WhenDisconnectedAsync"/> and
/// <see cref="ServerSocketExtensions.WhenDisconnectedAsync"/> verifying that a duplicate
/// <c>OnDisconnected</c> event does not cause the completion source to throw
/// <see cref="InvalidOperationException"/>. Mirrors the WebSocket tests.
/// </summary>
public class WhenDisconnectedAsyncTests
{
    /// <summary>
    /// Firing <c>OnDisconnected</c> twice on a client socket must not throw.
    /// </summary>
    [Fact]
    public async Task Client_WhenDisconnectedAsync_EventFiresTwice_NoThrow()
    {
        var socket = new FakeClientSocket();

        var task = socket.WhenDisconnectedAsync(TestContext.Current.CancellationToken);

        socket.RaiseDisconnected(SocketCloseStatus.ClosedLocal);
        socket.RaiseDisconnected(SocketCloseStatus.ClosedRemote);

        var status = await task;

        status.Is(SocketCloseStatus.ClosedLocal);
    }

    /// <summary>
    /// Firing <c>OnDisconnected</c> twice on a server socket must not throw.
    /// </summary>
    [Fact]
    public async Task Server_WhenDisconnectedAsync_EventFiresTwice_NoThrow()
    {
        var socket = new FakeServerSocket();

        var task = socket.WhenDisconnectedAsync(TestContext.Current.CancellationToken);

        socket.RaiseDisconnected(SocketCloseStatus.ClosedLocal);
        socket.RaiseDisconnected(SocketCloseStatus.ClosedRemote);

        var status = await task;

        status.Is(SocketCloseStatus.ClosedLocal);
    }

    /// <summary>
    /// Firing <c>OnConnected</c> twice on a client socket must not throw.
    /// </summary>
    [Fact]
    public async Task Client_WhenConnectedAsync_EventFiresTwice_NoThrow()
    {
        var socket = new FakeClientSocket();

        var task = socket.WhenConnectedAsync(TestContext.Current.CancellationToken);

        socket.RaiseConnected();
        socket.RaiseConnected();

        await task;
    }

    /// <summary>
    /// Minimal <see cref="IClientSocket"/> fake — only the event surface is wired; non-event
    /// members throw because the tests exercise the extension methods' subscription path only.
    /// </summary>
    private sealed class FakeClientSocket : IClientSocket
    {
        public ILogger Logger { get; } = VoidLogger.Instance;

        public bool IsConnected => false;

        public event Action? OnConnected;
        public event Action<SocketCloseStatus>? OnDisconnected;
        public event Action<Exception>? OnError
        {
            add { }
            remove { }
        }
        public event Action<ReadOnlyMemory<byte>>? OnReceived
        {
            add { }
            remove { }
        }

        public void RaiseConnected() => OnConnected?.Invoke();

        public void RaiseDisconnected(SocketCloseStatus status) => OnDisconnected?.Invoke(status);

        public void Connect(IPEndPoint endpoint, SslClientAuthenticationOptions? authOptions = null) =>
            throw new NotImplementedException();

        public void Disconnect() => throw new NotImplementedException();

        public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default) =>
            throw new NotImplementedException();

        public void Dispose() { }
    }

    /// <summary>
    /// Minimal <see cref="IServerSocket"/> fake with a raisable <c>OnDisconnected</c> event.
    /// </summary>
    private sealed class FakeServerSocket : IServerSocket
    {
        public ILogger Logger { get; } = VoidLogger.Instance;

        public event Action<SocketCloseStatus>? OnDisconnected;
        public event Action<Exception>? OnError
        {
            add { }
            remove { }
        }
        public event Action<ReadOnlyMemory<byte>>? OnReceived
        {
            add { }
            remove { }
        }

        public void RaiseDisconnected(SocketCloseStatus status) => OnDisconnected?.Invoke(status);

        public void Disconnect() => throw new NotImplementedException();

        public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default) =>
            throw new NotImplementedException();

        public void Dispose() { }
    }
}
