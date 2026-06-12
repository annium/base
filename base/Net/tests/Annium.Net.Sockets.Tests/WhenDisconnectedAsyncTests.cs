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
    /// <returns>A task that represents the asynchronous test.</returns>
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
    /// <returns>A task that represents the asynchronous test.</returns>
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
    /// <returns>A task that represents the asynchronous test.</returns>
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
        /// <summary>Gets the logger associated with this socket.</summary>
        public ILogger Logger { get; } = VoidLogger.Instance;

        /// <summary>Gets a value indicating whether the socket is currently connected.</summary>
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

        /// <summary>Raises the <c>OnConnected</c> event to simulate a successful connection.</summary>
        public void RaiseConnected() => OnConnected?.Invoke();

        /// <summary>Raises the <c>OnDisconnected</c> event with the given close status.</summary>
        /// <param name="status">The close status to report to subscribers.</param>
        public void RaiseDisconnected(SocketCloseStatus status) => OnDisconnected?.Invoke(status);

        /// <summary>Not implemented — this fake only exercises the event subscription path.</summary>
        /// <param name="endpoint">The remote endpoint to connect to.</param>
        /// <param name="authOptions">Optional SSL authentication options.</param>
        public void Connect(IPEndPoint endpoint, SslClientAuthenticationOptions? authOptions = null) =>
            throw new NotImplementedException();

        /// <summary>Not implemented — this fake only exercises the event subscription path.</summary>
        public void Disconnect() => throw new NotImplementedException();

        /// <summary>Not implemented — this fake only exercises the event subscription path.</summary>
        /// <param name="data">The data to send.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ValueTask{SocketSendStatus}"/> representing the pending send.</returns>
        public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default) =>
            throw new NotImplementedException();

        /// <summary>Disposes the fake socket (no-op).</summary>
        public void Dispose() { }
    }

    /// <summary>
    /// Minimal <see cref="IServerSocket"/> fake with a raisable <c>OnDisconnected</c> event.
    /// </summary>
    private sealed class FakeServerSocket : IServerSocket
    {
        /// <summary>Gets the logger associated with this server socket.</summary>
        public ILogger Logger { get; } = VoidLogger.Instance;

        /// <summary>Always reports connected — this fake only exercises the event subscription path.</summary>
        public bool IsConnected => true;

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

        /// <summary>Raises the <c>OnDisconnected</c> event with the given close status.</summary>
        /// <param name="status">The close status to report to subscribers.</param>
        public void RaiseDisconnected(SocketCloseStatus status) => OnDisconnected?.Invoke(status);

        /// <summary>Not implemented — this fake only exercises the event subscription path.</summary>
        public void Disconnect() => throw new NotImplementedException();

        /// <summary>Not implemented — this fake only exercises the event subscription path.</summary>
        /// <param name="data">The data to send.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ValueTask{SocketSendStatus}"/> representing the pending send.</returns>
        public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default) =>
            throw new NotImplementedException();

        /// <summary>Disposes the fake socket (no-op).</summary>
        public void Dispose() { }
    }
}
