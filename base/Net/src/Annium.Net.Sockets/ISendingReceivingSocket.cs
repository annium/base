namespace Annium.Net.Sockets;

/// <summary>
/// Represents a socket that can both send and receive data
/// </summary>
public interface ISendingReceivingSocket : ISendingSocket, IReceivingSocket;
