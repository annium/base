using System;

namespace Annium.Net.Sockets;

/// <summary>
/// Interface for sockets that can receive binary data
/// </summary>
public interface IReceivingSocket
{
    /// <summary>
    /// Event is invoked, when binary message arrives
    /// </summary>
    event Action<ReadOnlyMemory<byte>> OnReceived;
}
