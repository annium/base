using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests.Internal;

/// <summary>
/// Tests for <see cref="Annium.Net.Sockets.SendingSocketExtensions.SendTextAsync"/> over an
/// in-memory <see cref="MessagingManagedSocket"/>.
/// </summary>
public class SendingSocketExtensionsTests
{
    /// <summary>
    /// Sending an ASCII-plus-multibyte-Unicode string returns Ok, and the payload written
    /// to the underlying MemoryStream decodes (after stripping the 4-byte length-prefix
    /// framing added by MessagingManagedSocket) to the original UTF-8 bytes.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendTextAsync_AsciiAndUnicodeString_WritesUtf8PayloadAndReturnsOk()
    {
        // UTF-8: ASCII + 3-byte kanji + 4-byte emoji
        const string text = "hello 世界 \U0001f600";
        var expectedBytes = Encoding.UTF8.GetBytes(text);

        using var stream = new MemoryStream();
        using var socket = new MessagingManagedSocket(stream, ManagedSocketOptionsBase.Default, VoidLogger.Instance);

        var status = await socket.SendTextAsync(text, CancellationToken.None);

        status.Is(SocketSendStatus.Ok);

        // The messaging framing prepends a 4-byte little-endian payload-length header.
        var written = stream.ToArray();
        written.Length.Is(sizeof(int) + expectedBytes.Length);

        var framedLength = BitConverter.ToInt32(written, 0);
        framedLength.Is(expectedBytes.Length);

        var payload = written.AsSpan(sizeof(int)).ToArray();
        payload.IsEqual(expectedBytes);
    }

    /// <summary>
    /// Sending with a pre-cancelled token returns Canceled without writing to the stream.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendTextAsync_PreCancelledToken_ReturnsCanceled()
    {
        using var stream = new MemoryStream();
        using var socket = new MessagingManagedSocket(stream, ManagedSocketOptionsBase.Default, VoidLogger.Instance);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var status = await socket.SendTextAsync("anything", cts.Token);

        status.Is(SocketSendStatus.Canceled);

        // Nothing should have been written to the stream because the send was short-circuited.
        stream.Length.Is(0L);
    }

    /// <summary>
    /// Round-trip: the bytes received by a second MessagingManagedSocket reading from the same
    /// MemoryStream decode to the original text.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task SendTextAsync_RoundTrip_ReceivedBytesDecodeToOriginalText()
    {
        const string text = "pingé世";
        var expectedBytes = Encoding.UTF8.GetBytes(text);

        // Write side: sender → MemoryStream
        using var stream = new MemoryStream();
        using var sender = new MessagingManagedSocket(stream, ManagedSocketOptionsBase.Default, VoidLogger.Instance);
        await sender.SendTextAsync(text, CancellationToken.None);

        // Reset to start so the receiver can read the framed data.
        stream.Position = 0;

        ReadOnlyMemory<byte>? received = null;
        using var reader = new MessagingManagedSocket(stream, ManagedSocketOptionsBase.Default, VoidLogger.Instance);
        reader.OnReceived += msg => received = msg;

        // ListenAsync will return once the stream ends (ClosedRemote).
        await reader.ListenAsync(CancellationToken.None);

        received.IsNotNull();
        received!.Value.ToArray().IsEqual(expectedBytes);
    }
}
