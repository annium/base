using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests.Internal;

/// <summary>
/// Tests for the <see cref="MessagingManagedSocket.ListenAsync"/> disposed fast-path:
/// when the socket is already disposed, <c>ListenAsync</c> must return
/// <see cref="SocketCloseStatus.ClosedLocal"/> immediately without touching the stream.
/// </summary>
public class MessagingManagedSocketListenDisposedTests
{
    /// <summary>
    /// After <see cref="MessagingManagedSocket.Dispose"/> is called, a subsequent
    /// <see cref="MessagingManagedSocket.ListenAsync"/> must return immediately with
    /// <see cref="SocketCloseStatus.ClosedLocal"/> and no exception.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ListenAsync_AfterDispose_ReturnsClosedLocal()
    {
        await using var stream = new MemoryStream();
        var socket = new MessagingManagedSocket(stream, ManagedSocketOptionsBase.Default, VoidLogger.Instance);

        // MessagingManagedSocket implements only IDisposable (no DisposeAsync); suppress
        // the VSTHRD103 warning that fires because a DisposeAsync extension exists for IDisposable.
#pragma warning disable VSTHRD103
        socket.Dispose();
#pragma warning restore VSTHRD103

        var result = await socket.ListenAsync(CancellationToken.None);

        result.Status.Is(SocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();
    }

    /// <summary>
    /// After dispose, <c>ListenAsync</c> with a pre-cancelled token must still return
    /// <see cref="SocketCloseStatus.ClosedLocal"/> (the disposed check precedes the
    /// cancellation check).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ListenAsync_AfterDisposeWithCancelledToken_ReturnsClosedLocal()
    {
        await using var stream = new MemoryStream();
        var socket = new MessagingManagedSocket(stream, ManagedSocketOptionsBase.Default, VoidLogger.Instance);

#pragma warning disable VSTHRD103
        socket.Dispose();
#pragma warning restore VSTHRD103

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await socket.ListenAsync(cts.Token);

        result.Status.Is(SocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();
    }
}
