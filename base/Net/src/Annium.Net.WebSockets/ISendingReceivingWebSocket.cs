namespace Annium.Net.WebSockets;

/// <summary>
/// Represents a WebSocket that can both send and receive messages
/// </summary>
public interface ISendingReceivingWebSocket : ISendingWebSocket, IReceivingWebSocket;
