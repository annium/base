using System;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Static class containing predefined protocol frames for WebSocket communication
/// </summary>
public static class ProtocolFrames
{
    /// <summary>
    /// Gets the ping frame data used for connection monitoring
    /// </summary>
    public static ReadOnlyMemory<byte> Ping { get; } = new byte[] { 0xFF };
}
