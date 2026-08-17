using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests;

/// <summary>
/// Tests for <see cref="ClientSocketExtensions"/>.
/// </summary>
public class ClientSocketExtensionsTests
{
    // -------------------------------------------------------------------------
    // Connect(Uri) — TG5
    // -------------------------------------------------------------------------

    /// <summary>
    /// Connecting via a loopback URI passes an <see cref="IPEndPoint"/> at the correct port
    /// to the underlying <see cref="IClientSocket.Connect(IPEndPoint, SslClientAuthenticationOptions?)"/>.
    /// </summary>
    [Fact]
    public void Connect_LoopbackUri_PassesCorrectPort()
    {
        const int port = 19876;
        var uri = new Uri($"http://127.0.0.1:{port}");
        var fake = new RecordingClientSocket();

        fake.Connect(uri);

        fake.ConnectedEndpoints.Has(1);
        fake.ConnectedEndpoints.At(0).Port.Is(port);
    }

    /// <summary>
    /// Connecting via a loopback URI resolves an address and forwards it to the socket.
    /// </summary>
    [Fact]
    public void Connect_LoopbackUri_ResolvesAddress()
    {
        var uri = new Uri("http://127.0.0.1:11111");
        var fake = new RecordingClientSocket();

        fake.Connect(uri);

        fake.ConnectedEndpoints.Has(1);
        fake.ConnectedEndpoints.At(0).Address.Is(IPAddress.Loopback);
    }

    // -------------------------------------------------------------------------
    // WhenConnectedAsync cancellation — TG7
    // -------------------------------------------------------------------------

    /// <summary>
    /// When the cancellation token is cancelled before OnConnected fires,
    /// <see cref="ClientSocketExtensions.WhenConnectedAsync"/> throws
    /// <see cref="OperationCanceledException"/> and the handler is unsubscribed.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenConnectedAsync_TokenCancelledBeforeConnected_ThrowsAndUnsubscribes()
    {
        var fake = new FakeClientSocket();
        using var cts = new CancellationTokenSource();

        var waitTask = fake.WhenConnectedAsync(cts.Token);

        // Cancel the token before the OnConnected event is ever raised.
        await cts.CancelAsync();

        // The task must fault with OperationCanceledException. waitTask was started before the
        // cancel (to exercise cancel-after-subscribe), so it is awaited directly here.
        var threw = false;
        try
        {
            await waitTask;
        }
        catch (OperationCanceledException)
        {
            threw = true;
        }

        threw.IsTrue();

        // After cancellation the OnConnected handler should be unsubscribed so
        // raising the event no longer reaches the (now-abandoned) TCS.
        fake.HasConnectedSubscribers.IsFalse();
    }

    /// <summary>
    /// When the cancellation token is already cancelled at call time,
    /// <see cref="ClientSocketExtensions.WhenConnectedAsync"/> throws immediately
    /// without leaking the subscription.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenConnectedAsync_PreCancelledToken_ThrowsImmediatelyAndUnsubscribes()
    {
        var fake = new FakeClientSocket();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Wrap.It(async () => await fake.WhenConnectedAsync(cts.Token)).ThrowsAsync<OperationCanceledException>();
        fake.HasConnectedSubscribers.IsFalse();
    }

    /// <summary>
    /// After cancellation, raising OnConnected on the fake has no effect (subscription
    /// was cleaned up). This verifies the finally-unsubscribe path does not leave a dangling
    /// handler on the socket's multicast list.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenConnectedAsync_AfterCancellation_RaisingConnectedHasNoEffect()
    {
        var fake = new FakeClientSocket();
        using var cts = new CancellationTokenSource();

        var waitTask = fake.WhenConnectedAsync(cts.Token);
        await cts.CancelAsync();

        try
        {
            await waitTask;
        }
        catch (OperationCanceledException)
        {
            // expected
        }

        // Raise connected — must not throw and the subscriber count stays at zero.
        fake.RaiseConnected();
        fake.HasConnectedSubscribers.IsFalse();
    }

    // -------------------------------------------------------------------------
    // WhenDisconnectedAsync cancellation — TG11
    // -------------------------------------------------------------------------

    /// <summary>
    /// When the cancellation token is cancelled before OnDisconnected fires,
    /// <see cref="ClientSocketExtensions.WhenDisconnectedAsync"/> throws
    /// <see cref="OperationCanceledException"/> and the handler is unsubscribed.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenDisconnectedAsync_TokenCancelledBeforeDisconnected_ThrowsAndUnsubscribes()
    {
        var fake = new FakeClientSocketWithDisconnect();
        using var cts = new CancellationTokenSource();

        var waitTask = fake.WhenDisconnectedAsync(cts.Token);

        // Cancel the token before OnDisconnected is ever raised.
        await cts.CancelAsync();

        // waitTask was started before the cancel (exercises cancel-after-subscribe),
        // so it is awaited directly here — manual try/catch to avoid VSTHRD003.
        var threw = false;
        try
        {
            await waitTask;
        }
        catch (OperationCanceledException)
        {
            threw = true;
        }

        threw.IsTrue();

        // After cancellation the OnDisconnected handler should be unsubscribed.
        fake.HasDisconnectedSubscribers.IsFalse();
    }

    /// <summary>
    /// When the cancellation token is already cancelled at call time,
    /// <see cref="ClientSocketExtensions.WhenDisconnectedAsync"/> throws immediately
    /// without leaking the subscription.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenDisconnectedAsync_PreCancelledToken_ThrowsImmediatelyAndUnsubscribes()
    {
        var fake = new FakeClientSocketWithDisconnect();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Wrap.It(async () => await fake.WhenDisconnectedAsync(cts.Token))
            .ThrowsAsync<OperationCanceledException>();
        fake.HasDisconnectedSubscribers.IsFalse();
    }

    /// <summary>
    /// After cancellation, raising OnDisconnected on the fake has no effect — the subscription
    /// was cleaned up and no dangling handler remains.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenDisconnectedAsync_AfterCancellation_RaisingDisconnectedHasNoEffect()
    {
        var fake = new FakeClientSocketWithDisconnect();
        using var cts = new CancellationTokenSource();

        var waitTask = fake.WhenDisconnectedAsync(cts.Token);
        await cts.CancelAsync();

        try
        {
            await waitTask;
        }
        catch (OperationCanceledException)
        {
            // expected
        }

        // Raise disconnected — must not throw and the subscriber count stays at zero.
        fake.RaiseDisconnected(SocketCloseStatus.ClosedLocal);
        fake.HasDisconnectedSubscribers.IsFalse();
    }

    // -------------------------------------------------------------------------
    // Fakes
    // -------------------------------------------------------------------------

    /// <summary>
    /// Minimal <see cref="IClientSocket"/> fake that records endpoints passed to
    /// <see cref="Connect(IPEndPoint, SslClientAuthenticationOptions?)"/>.
    /// </summary>
    private sealed class RecordingClientSocket : IClientSocket
    {
        /// <summary>Gets the logger used by this fake.</summary>
        public ILogger Logger { get; } = VoidLogger.Instance;

        /// <summary>Always false — this fake is not connected.</summary>
        public bool IsConnected => false;

        /// <summary>Never raised — declared to satisfy the interface; this fake does not signal that the socket connects.</summary>
        public event Action? OnConnected
        {
            add { }
            remove { }
        }

        /// <summary>Never raised — declared to satisfy the interface; this fake does not signal that the socket disconnects.</summary>
        public event Action<SocketCloseStatus>? OnDisconnected
        {
            add { }
            remove { }
        }

        /// <summary>Never raised — declared to satisfy the interface; this fake does not signal that the socket reports an error.</summary>
        public event Action<Exception>? OnError
        {
            add { }
            remove { }
        }

        /// <summary>Never raised — declared to satisfy the interface; this fake does not signal that the socket receives data.</summary>
        public event Action<ReadOnlyMemory<byte>>? OnReceived
        {
            add { }
            remove { }
        }

        /// <summary>All endpoints passed to <c>Connect</c>.</summary>
        public List<IPEndPoint> ConnectedEndpoints { get; } = [];

        /// <summary>Records <paramref name="endpoint"/> so tests can assert the forwarded address and port.</summary>
        /// <param name="endpoint">The remote endpoint passed to the extension method.</param>
        /// <param name="authOptions">Optional SSL authentication options (ignored by this fake).</param>
        public void Connect(IPEndPoint endpoint, SslClientAuthenticationOptions? authOptions = null) =>
            ConnectedEndpoints.Add(endpoint);

        /// <summary>No-op — this fake does not implement disconnect.</summary>
        public void Disconnect() { }

        /// <summary>Not implemented — this fake only exercises the <c>Connect</c> path.</summary>
        /// <param name="data">The data to send.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ValueTask{SocketSendStatus}"/> representing the pending send.</returns>
        public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default) =>
            ValueTask.FromResult(SocketSendStatus.Ok);

        /// <summary>Disposes the fake socket (no-op).</summary>
        public void Dispose() { }
    }

    /// <summary>
    /// Minimal <see cref="IClientSocket"/> fake with a raisable <see cref="OnConnected"/> event
    /// and a subscriber-count helper for TG7.
    /// </summary>
    private sealed class FakeClientSocket : IClientSocket
    {
        /// <summary>Gets the logger used by this fake.</summary>
        public ILogger Logger { get; } = VoidLogger.Instance;

        /// <summary>Always false — this fake is never actually connected.</summary>
        public bool IsConnected => false;

        /// <summary>Backing delegate for <see cref="OnConnected"/>.</summary>
        private event Action? _onConnected;

        /// <summary>Event raised when the socket connects successfully.</summary>
        public event Action OnConnected
        {
            add => _onConnected += value;
            remove => _onConnected -= value;
        }

        /// <summary>Never raised — declared to satisfy the interface; this fake does not signal that the socket disconnects.</summary>
        public event Action<SocketCloseStatus>? OnDisconnected
        {
            add { }
            remove { }
        }

        /// <summary>Never raised — declared to satisfy the interface; this fake does not signal that the socket reports an error.</summary>
        public event Action<Exception>? OnError
        {
            add { }
            remove { }
        }

        /// <summary>Never raised — declared to satisfy the interface; this fake does not signal that the socket receives data.</summary>
        public event Action<ReadOnlyMemory<byte>>? OnReceived
        {
            add { }
            remove { }
        }

        /// <summary>
        /// <see langword="true"/> when at least one handler is subscribed to <see cref="OnConnected"/>.
        /// </summary>
        public bool HasConnectedSubscribers => _onConnected is not null;

        /// <summary>Raises <see cref="OnConnected"/>.</summary>
        public void RaiseConnected() => _onConnected?.Invoke();

        /// <summary>Not implemented — this fake only exercises the <c>OnConnected</c> subscription path.</summary>
        /// <param name="endpoint">The remote endpoint to connect to.</param>
        /// <param name="authOptions">Optional SSL authentication options.</param>
        public void Connect(IPEndPoint endpoint, SslClientAuthenticationOptions? authOptions = null) =>
            throw new NotImplementedException();

        /// <summary>Not implemented — this fake only exercises the <c>OnConnected</c> subscription path.</summary>
        public void Disconnect() => throw new NotImplementedException();

        /// <summary>Not implemented — this fake only exercises the <c>OnConnected</c> subscription path.</summary>
        /// <param name="data">The data to send.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ValueTask{SocketSendStatus}"/> representing the pending send.</returns>
        public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default) =>
            throw new NotImplementedException();

        /// <summary>Disposes the fake socket (no-op).</summary>
        public void Dispose() { }
    }

    /// <summary>
    /// Minimal <see cref="IClientSocket"/> fake with a raisable <see cref="OnDisconnected"/> event
    /// and a subscriber-count helper for TG11.
    /// </summary>
    private sealed class FakeClientSocketWithDisconnect : IClientSocket
    {
        /// <summary>Gets the logger used by this fake.</summary>
        public ILogger Logger { get; } = VoidLogger.Instance;

        /// <summary>Always false — this fake is never actually connected.</summary>
        public bool IsConnected => false;

        /// <summary>Never raised — declared to satisfy the interface; this fake does not signal that the socket connects.</summary>
        public event Action OnConnected
        {
            add { }
            remove { }
        }

        /// <summary>Backing delegate for <see cref="OnDisconnected"/>.</summary>
        private event Action<SocketCloseStatus>? _onDisconnected;

        /// <summary>Event raised when the socket disconnects, carrying the close status.</summary>
        public event Action<SocketCloseStatus> OnDisconnected
        {
            add => _onDisconnected += value;
            remove => _onDisconnected -= value;
        }

        /// <summary>Never raised — declared to satisfy the interface; this fake does not signal that the socket reports an error.</summary>
        public event Action<Exception>? OnError
        {
            add { }
            remove { }
        }

        /// <summary>Never raised — declared to satisfy the interface; this fake does not signal that the socket receives data.</summary>
        public event Action<ReadOnlyMemory<byte>>? OnReceived
        {
            add { }
            remove { }
        }

        /// <summary>
        /// <see langword="true"/> when at least one handler is subscribed to <see cref="OnDisconnected"/>.
        /// </summary>
        public bool HasDisconnectedSubscribers => _onDisconnected is not null;

        /// <summary>Raises <see cref="OnDisconnected"/> with the given status.</summary>
        /// <param name="status">The close status to deliver to subscribers.</param>
        public void RaiseDisconnected(SocketCloseStatus status) => _onDisconnected?.Invoke(status);

        /// <summary>Not implemented — this fake only exercises the <c>OnDisconnected</c> subscription path.</summary>
        /// <param name="endpoint">The remote endpoint to connect to.</param>
        /// <param name="authOptions">Optional SSL authentication options.</param>
        public void Connect(IPEndPoint endpoint, SslClientAuthenticationOptions? authOptions = null) =>
            throw new NotImplementedException();

        /// <summary>Not implemented — this fake only exercises the <c>OnDisconnected</c> subscription path.</summary>
        public void Disconnect() => throw new NotImplementedException();

        /// <summary>Not implemented — this fake only exercises the <c>OnDisconnected</c> subscription path.</summary>
        /// <param name="data">The data to send.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <see cref="ValueTask{SocketSendStatus}"/> representing the pending send.</returns>
        public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default) =>
            throw new NotImplementedException();

        /// <summary>Disposes the fake socket (no-op).</summary>
        public void Dispose() { }
    }
}
