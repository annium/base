using System;

namespace Annium.Net.Sockets;

public interface IReceivingSocket
{
    /// <summary>
    /// Event is invoked, when binary message arrives
    /// </summary>
    event Action<ReadOnlyMemory<byte>> Received;
}