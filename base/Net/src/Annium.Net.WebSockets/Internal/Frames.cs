using System;

namespace Annium.Net.WebSockets.Internal;

public static class ProtocolFrames
{
    public static ReadOnlyMemory<byte> Ping { get; } = new byte[] { 0xFF };
}
