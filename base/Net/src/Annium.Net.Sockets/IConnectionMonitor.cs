using System;

namespace Annium.Net.Sockets;

/// <summary>
/// Monitors a socket connection and raises <see cref="OnConnectionLost"/> when the connection is
/// detected as lost. The default implementation (<see cref="ConnectionMonitorBase"/>) centralizes the
/// start/stop idempotency invariant; custom monitors deriving from it inherit that guarantee.
/// </summary>
public interface IConnectionMonitor
{
    /// <summary>
    /// Event raised when the connection is detected as lost.
    /// </summary>
    event Action OnConnectionLost;

    /// <summary>
    /// Starts the connection monitor.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the connection monitor.
    /// </summary>
    void Stop();
}
