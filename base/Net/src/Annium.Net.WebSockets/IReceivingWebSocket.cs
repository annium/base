using System;

namespace Annium.Net.WebSockets;

public interface IReceivingWebSocket
{
    /// <summary>
    /// Event is invoked, when text message arrives
    /// </summary>
    event Action<ReadOnlyMemory<byte>> TextReceived;

    /// <summary>
    /// Event is invoked, when binary message arrives
    /// </summary>
    event Action<ReadOnlyMemory<byte>> BinaryReceived;
}