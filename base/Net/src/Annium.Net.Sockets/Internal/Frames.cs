using System;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Contains protocol frames used for socket communication.
/// </summary>
internal static class ProtocolFrames
{
    /// <summary>
    /// Gets the ping frame used for connection monitoring.
    /// </summary>
    public static ReadOnlyMemory<byte> Ping { get; } = new byte[] { 0xFF };

    /// <summary>
    /// Returns whether the given data span equals the ping frame.
    /// </summary>
    /// <param name="data">The data to compare against the ping frame.</param>
    /// <returns>True when the data equals the ping frame, false otherwise.</returns>
    public static bool IsPingFrame(ReadOnlyMemory<byte> data) => data.Span.SequenceEqual(Ping.Span);
}
