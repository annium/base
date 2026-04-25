using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Net.WebSockets.Tests;

/// <summary>
/// Regression tests for <see cref="ClientWebSocketExtensions.WhenDisconnectedAsync"/> and
/// <see cref="ServerWebSocketExtensions.WhenDisconnectedAsync"/> verifying that a duplicate
/// <c>OnDisconnected</c> event does not cause the completion source to throw
/// <see cref="InvalidOperationException"/>.
/// </summary>
public class WhenDisconnectedAsyncTests
{
    /// <summary>
    /// Firing <c>OnDisconnected</c> twice on a client socket must not throw — the handler in
    /// <see cref="ClientWebSocketExtensions.WhenDisconnectedAsync"/> unsubscribes itself after the
    /// first fire, but a racing second fire before unsubscribe is observable and must be tolerated.
    /// </summary>
    [Fact]
    public async Task Client_WhenDisconnectedAsync_EventFiresTwice_NoThrow()
    {
        var socket = new FakeClientWebSocket();

        var task = socket.WhenDisconnectedAsync(TestContext.Current.CancellationToken);

        socket.RaiseDisconnected(WebSocketCloseStatus.ClosedLocal);
        socket.RaiseDisconnected(WebSocketCloseStatus.ClosedRemote);

        var status = await task;

        status.Is(WebSocketCloseStatus.ClosedLocal);
    }

    /// <summary>
    /// Firing <c>OnDisconnected</c> twice on a server socket must not throw — same guard as the
    /// client extension.
    /// </summary>
    [Fact]
    public async Task Server_WhenDisconnectedAsync_EventFiresTwice_NoThrow()
    {
        var socket = new FakeServerWebSocket();

        var task = socket.WhenDisconnectedAsync(TestContext.Current.CancellationToken);

        socket.RaiseDisconnected(WebSocketCloseStatus.ClosedLocal);
        socket.RaiseDisconnected(WebSocketCloseStatus.ClosedRemote);

        var status = await task;

        status.Is(WebSocketCloseStatus.ClosedLocal);
    }

    /// <summary>
    /// Firing <c>OnConnected</c> twice on a client socket must not throw.
    /// </summary>
    [Fact]
    public async Task Client_WhenConnectedAsync_EventFiresTwice_NoThrow()
    {
        var socket = new FakeClientWebSocket();

        var task = socket.WhenConnectedAsync(TestContext.Current.CancellationToken);

        socket.RaiseConnected();
        socket.RaiseConnected();

        await task;
    }

    /// <summary>
    /// Minimal <see cref="IClientWebSocket"/> fake exposing event raise methods. Non-event surface
    /// members throw <see cref="NotImplementedException"/> because the tests only exercise the
    /// event subscription path in the extension methods.
    /// </summary>
    private sealed class FakeClientWebSocket : IClientWebSocket
    {
        public ILogger Logger { get; } = VoidLogger.Instance;

        public bool IsConnected => false;

        public event Action? OnConnected;
        public event Action<WebSocketCloseStatus>? OnDisconnected;
        public event Action<Exception>? OnError
        {
            add { }
            remove { }
        }
        public event Action<ReadOnlyMemory<byte>>? OnTextReceived
        {
            add { }
            remove { }
        }
        public event Action<ReadOnlyMemory<byte>>? OnBinaryReceived
        {
            add { }
            remove { }
        }

        public void RaiseConnected() => OnConnected?.Invoke();

        public void RaiseDisconnected(WebSocketCloseStatus status) => OnDisconnected?.Invoke(status);

        public void Connect(Uri uri) => throw new NotImplementedException();

        public void Disconnect() => throw new NotImplementedException();

        public ValueTask<WebSocketSendStatus> SendTextAsync(
            ReadOnlyMemory<byte> text,
            CancellationToken ct = default
        ) => throw new NotImplementedException();

        public ValueTask<WebSocketSendStatus> SendBinaryAsync(
            ReadOnlyMemory<byte> data,
            CancellationToken ct = default
        ) => throw new NotImplementedException();

        public void Dispose() { }
    }

    /// <summary>
    /// Minimal <see cref="IServerWebSocket"/> fake exposing event raise methods.
    /// </summary>
    private sealed class FakeServerWebSocket : IServerWebSocket
    {
        public ILogger Logger { get; } = VoidLogger.Instance;

        public event Action<WebSocketCloseStatus>? OnDisconnected;
        public event Action<Exception>? OnError
        {
            add { }
            remove { }
        }
        public event Action<ReadOnlyMemory<byte>>? OnTextReceived
        {
            add { }
            remove { }
        }
        public event Action<ReadOnlyMemory<byte>>? OnBinaryReceived
        {
            add { }
            remove { }
        }

        public void RaiseDisconnected(WebSocketCloseStatus status) => OnDisconnected?.Invoke(status);

        public void Disconnect() => throw new NotImplementedException();

        public ValueTask<WebSocketSendStatus> SendTextAsync(
            ReadOnlyMemory<byte> text,
            CancellationToken ct = default
        ) => throw new NotImplementedException();

        public ValueTask<WebSocketSendStatus> SendBinaryAsync(
            ReadOnlyMemory<byte> data,
            CancellationToken ct = default
        ) => throw new NotImplementedException();
    }
}
