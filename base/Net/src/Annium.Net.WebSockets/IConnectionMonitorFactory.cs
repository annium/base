namespace Annium.Net.WebSockets;

/// <summary>
/// Factory for creating connection monitors to manage WebSocket connection health
/// </summary>
public interface IConnectionMonitorFactory
{
    /// <summary>
    /// Creates a connection monitor for the specified WebSocket with given options
    /// </summary>
    /// <param name="socket">The WebSocket to monitor</param>
    /// <param name="options">Configuration options for the connection monitor</param>
    /// <returns>A connection monitor instance</returns>
    ConnectionMonitorBase Create(ISendingReceivingWebSocket socket, ConnectionMonitorOptions options);
}
