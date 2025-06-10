using System;

namespace Annium.Net.WebSockets;

/// <summary>
/// Interface for WebSocket components that can receive messages from remote endpoints
/// </summary>
public interface IReceivingWebSocket
{
    /// <summary>
    /// Event is invoked, when text message arrives
    /// </summary>
    event Action<ReadOnlyMemory<byte>> OnTextReceived;

    /// <summary>
    /// Event is invoked, when binary message arrives
    /// </summary>
    event Action<ReadOnlyMemory<byte>> OnBinaryReceived;
}
