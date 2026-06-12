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
    /// <returns>A task that represents the asynchronous test.</returns>
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
    /// <returns>A task that represents the asynchronous test.</returns>
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
    /// <returns>A task that represents the asynchronous test.</returns>
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
    /// When the CancellationToken is already cancelled before <c>OnDisconnected</c> fires,
    /// <see cref="ClientWebSocketExtensions.WhenDisconnectedAsync"/> must throw
    /// <see cref="OperationCanceledException"/> and must NOT leave a handler subscribed —
    /// raising <c>OnDisconnected</c> after the cancelled wait must not throw or produce
    /// any side-effect from the leaked handler.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenDisconnectedAsync_CtCancelledBeforeEvent_ThrowsAndUnsubscribes()
    {
        var socket = new FakeClientWebSocket();

        var alreadyCancelled = new CancellationToken(canceled: true);
        var task = socket.WhenDisconnectedAsync(alreadyCancelled);

        // The task must fault with OperationCanceledException.
        var threw = false;
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            threw = true;
        }

        threw.IsTrue();

        // The handler must have been removed in the finally block — this is the leak-prevention
        // contract. Asserting the subscriber is gone is what would catch a removed finally-unsubscribe
        // (raising the event alone never throws, leaked handler or not).
        socket.HasDisconnectedSubscribers.IsFalse();

        // Raising OnDisconnected after the cancelled wait must be a no-op (no stale handler present).
        socket.RaiseDisconnected(WebSocketCloseStatus.ClosedRemote);
    }

    /// <summary>
    /// When the CancellationToken is already cancelled before <c>OnConnected</c> fires,
    /// <see cref="ClientWebSocketExtensions.WhenConnectedAsync"/> must throw
    /// <see cref="OperationCanceledException"/> and must NOT leave a handler subscribed —
    /// raising <c>OnConnected</c> after the cancelled wait must not throw or produce
    /// any side-effect from the leaked handler.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenConnectedAsync_CtCancelledBeforeEvent_ThrowsAndUnsubscribes()
    {
        var socket = new FakeClientWebSocket();

        var alreadyCancelled = new CancellationToken(canceled: true);
        var task = socket.WhenConnectedAsync(alreadyCancelled);

        // The task must fault with OperationCanceledException.
        var threw = false;
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            threw = true;
        }

        threw.IsTrue();

        // The handler must have been removed in the finally block — this is the leak-prevention
        // contract. Asserting the subscriber is gone is what would catch a removed finally-unsubscribe
        // (raising the event alone never throws, leaked handler or not).
        socket.HasConnectedSubscribers.IsFalse();

        // Raising OnConnected after the cancelled wait must be a no-op (no stale handler present).
        socket.RaiseConnected();
    }

    /// <summary>
    /// When the CancellationToken is already cancelled before <c>OnDisconnected</c> fires,
    /// <see cref="ServerWebSocketExtensions.WhenDisconnectedAsync"/> must throw
    /// <see cref="OperationCanceledException"/> and must NOT leave a handler subscribed —
    /// raising <c>OnDisconnected</c> after the cancelled wait must not throw or produce
    /// any side-effect from the leaked handler.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Server_WhenDisconnectedAsync_CtCancelledBeforeEvent_ThrowsAndUnsubscribes()
    {
        var socket = new FakeServerWebSocket();

        var alreadyCancelled = new CancellationToken(canceled: true);
        var task = socket.WhenDisconnectedAsync(alreadyCancelled);

        // The task must fault with OperationCanceledException.
        var threw = false;
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            threw = true;
        }

        threw.IsTrue();

        // The handler must have been removed in the finally block — this is the leak-prevention
        // contract. Asserting the subscriber is gone is what would catch a removed finally-unsubscribe
        // (raising the event alone never throws, leaked handler or not).
        socket.HasDisconnectedSubscribers.IsFalse();

        // Raising OnDisconnected after the cancelled wait must be a no-op (no stale handler present).
        socket.RaiseDisconnected(WebSocketCloseStatus.ClosedRemote);
    }

    /// <summary>
    /// Minimal <see cref="IClientWebSocket"/> fake exposing event raise methods. Non-event surface
    /// members throw <see cref="NotImplementedException"/> because the tests only exercise the
    /// event subscription path in the extension methods.
    /// </summary>
    private sealed class FakeClientWebSocket : IClientWebSocket
    {
        /// <summary>Gets the no-op logger for this fake socket.</summary>
        public ILogger Logger { get; } = VoidLogger.Instance;

        /// <summary>Always returns <see langword="false"/>; connection state is not simulated.</summary>
        public bool IsConnected => false;

        public event Action? OnConnected;
        public event Action<WebSocketCloseStatus>? OnDisconnected;
        public event Action<Exception>? OnError
        {
            add { }
            remove { }
        }

        /// <summary>Gets a value indicating whether any handler is subscribed to <c>OnConnected</c>.</summary>
        public bool HasConnectedSubscribers => OnConnected is not null;

        /// <summary>Gets a value indicating whether any handler is subscribed to <c>OnDisconnected</c>.</summary>
        public bool HasDisconnectedSubscribers => OnDisconnected is not null;

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

        /// <summary>Fires <see cref="IClientWebSocket.OnConnected"/> to simulate a connection event.</summary>
        public void RaiseConnected() => OnConnected?.Invoke();

        /// <summary>Fires <see cref="IClientWebSocket.OnDisconnected"/> with the given close status.</summary>
        /// <param name="status">The close status to deliver to subscribers.</param>
        public void RaiseDisconnected(WebSocketCloseStatus status) => OnDisconnected?.Invoke(status);

        /// <summary>Not implemented — tests do not exercise the connection path.</summary>
        /// <param name="uri">The URI to connect to (unused).</param>
        public void Connect(Uri uri) => throw new NotImplementedException();

        /// <summary>Not implemented — tests do not exercise the disconnect path.</summary>
        public void Disconnect() => throw new NotImplementedException();

        /// <summary>Not implemented — tests do not exercise the send path.</summary>
        /// <param name="text">The text payload (unused).</param>
        /// <param name="ct">Cancellation token (unused).</param>
        /// <returns>Never returns; always throws.</returns>
        public ValueTask<WebSocketSendStatus> SendTextAsync(
            ReadOnlyMemory<byte> text,
            CancellationToken ct = default
        ) => throw new NotImplementedException();

        /// <summary>Not implemented — tests do not exercise the send path.</summary>
        /// <param name="data">The binary payload (unused).</param>
        /// <param name="ct">Cancellation token (unused).</param>
        /// <returns>Never returns; always throws.</returns>
        public ValueTask<WebSocketSendStatus> SendBinaryAsync(
            ReadOnlyMemory<byte> data,
            CancellationToken ct = default
        ) => throw new NotImplementedException();

        /// <summary>No-op dispose; fake holds no resources.</summary>
        public void Dispose() { }
    }

    /// <summary>
    /// Minimal <see cref="IServerWebSocket"/> fake exposing event raise methods.
    /// </summary>
    private sealed class FakeServerWebSocket : IServerWebSocket
    {
        /// <summary>Gets the no-op logger for this fake socket.</summary>
        public ILogger Logger { get; } = VoidLogger.Instance;

        /// <summary>Gets a value indicating whether the fake socket is connected (always false).</summary>
        public bool IsConnected => false;

        public event Action<WebSocketCloseStatus>? OnDisconnected;

        /// <summary>Gets a value indicating whether any handler is subscribed to <c>OnDisconnected</c>.</summary>
        public bool HasDisconnectedSubscribers => OnDisconnected is not null;
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

        /// <summary>No-op dispose; fake holds no resources.</summary>
        public void Dispose() { }

        /// <summary>Fires <see cref="IServerWebSocket.OnDisconnected"/> with the given close status.</summary>
        /// <param name="status">The close status to deliver to subscribers.</param>
        public void RaiseDisconnected(WebSocketCloseStatus status) => OnDisconnected?.Invoke(status);

        /// <summary>Not implemented — tests do not exercise the disconnect path.</summary>
        public void Disconnect() => throw new NotImplementedException();

        /// <summary>Not implemented — tests do not exercise the send path.</summary>
        /// <param name="text">The text payload (unused).</param>
        /// <param name="ct">Cancellation token (unused).</param>
        /// <returns>Never returns; always throws.</returns>
        public ValueTask<WebSocketSendStatus> SendTextAsync(
            ReadOnlyMemory<byte> text,
            CancellationToken ct = default
        ) => throw new NotImplementedException();

        /// <summary>Not implemented — tests do not exercise the send path.</summary>
        /// <param name="data">The binary payload (unused).</param>
        /// <param name="ct">Cancellation token (unused).</param>
        /// <returns>Never returns; always throws.</returns>
        public ValueTask<WebSocketSendStatus> SendBinaryAsync(
            ReadOnlyMemory<byte> data,
            CancellationToken ct = default
        ) => throw new NotImplementedException();
    }
}
