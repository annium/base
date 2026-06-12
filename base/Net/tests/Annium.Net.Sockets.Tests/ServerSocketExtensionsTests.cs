using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests;

/// <summary>
/// Tests for <see cref="ServerSocketExtensions"/>.
/// </summary>
public class ServerSocketExtensionsTests
{
    // -------------------------------------------------------------------------
    // WhenDisconnectedAsync cancellation — TG11 (server variant)
    // -------------------------------------------------------------------------

    /// <summary>
    /// When the cancellation token is cancelled before OnDisconnected fires,
    /// <see cref="ServerSocketExtensions.WhenDisconnectedAsync"/> throws
    /// <see cref="OperationCanceledException"/> and the handler is unsubscribed.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenDisconnectedAsync_TokenCancelledBeforeDisconnected_ThrowsAndUnsubscribes()
    {
        var fake = new FakeServerSocket();
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
    /// <see cref="ServerSocketExtensions.WhenDisconnectedAsync"/> throws immediately
    /// without leaking the subscription.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WhenDisconnectedAsync_PreCancelledToken_ThrowsImmediatelyAndUnsubscribes()
    {
        var fake = new FakeServerSocket();
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
        var fake = new FakeServerSocket();
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
        fake.RaiseDisconnected(SocketCloseStatus.ClosedRemote);
        fake.HasDisconnectedSubscribers.IsFalse();
    }

    // -------------------------------------------------------------------------
    // Fake
    // -------------------------------------------------------------------------

    /// <summary>
    /// Minimal <see cref="IServerSocket"/> fake with a raisable <see cref="OnDisconnected"/> event
    /// and a subscriber-count helper for TG11.
    /// </summary>
    private sealed class FakeServerSocket : IServerSocket
    {
        /// <summary>Gets the logger used by this fake.</summary>
        public ILogger Logger { get; } = VoidLogger.Instance;

        /// <summary>Always false — this fake is never actually connected.</summary>
        public bool IsConnected => false;

        private event Action<SocketCloseStatus>? _onDisconnected;

        /// <summary>Raised when the socket disconnects; routes through the backing field so subscriber count is observable.</summary>
        public event Action<SocketCloseStatus> OnDisconnected
        {
            add => _onDisconnected += value;
            remove => _onDisconnected -= value;
        }

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

        /// <summary>
        /// <see langword="true"/> when at least one handler is subscribed to <see cref="OnDisconnected"/>.
        /// </summary>
        public bool HasDisconnectedSubscribers => _onDisconnected is not null;

        /// <summary>Raises the <c>OnDisconnected</c> event with the given close status.</summary>
        /// <param name="status">The close status to pass to subscribers.</param>
        public void RaiseDisconnected(SocketCloseStatus status) => _onDisconnected?.Invoke(status);

        /// <summary>Not implemented — this fake only exercises the event subscription path.</summary>
        public void Disconnect() => throw new NotImplementedException();

        /// <summary>Not implemented — this fake only exercises the event subscription path.</summary>
        /// <param name="data">The data to send.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A <c>ValueTask&lt;SocketSendStatus&gt;</c> representing the pending send.</returns>
        public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default) =>
            throw new NotImplementedException();

        /// <summary>Disposes the fake socket (no-op).</summary>
        public void Dispose() { }
    }
}
