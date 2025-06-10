namespace Annium.Net.Sockets;

/// <summary>
/// Factory interface for creating connection monitors
/// </summary>
public interface IConnectionMonitorFactory
{
    /// <summary>
    /// Creates a connection monitor for the specified socket
    /// </summary>
    /// <param name="socket">The socket to monitor</param>
    /// <param name="options">Configuration options for the monitor</param>
    /// <returns>A connection monitor instance</returns>
    ConnectionMonitorBase Create(ISendingReceivingSocket socket, ConnectionMonitorOptions options);
}
