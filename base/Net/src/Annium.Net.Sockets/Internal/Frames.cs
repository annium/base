using System;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Contains protocol frames used for socket communication
/// </summary>
public static class ProtocolFrames
{
    /// <summary>
    /// Gets the ping frame used for connection monitoring
    /// </summary>
    public static ReadOnlyMemory<byte> Ping { get; } = new byte[] { 0xFF };
}
