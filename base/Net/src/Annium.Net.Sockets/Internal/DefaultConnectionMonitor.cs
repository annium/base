using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Threading;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Default implementation of connection monitor that sends ping frames and monitors connection health
/// </summary>
internal class DefaultConnectionMonitor : ConnectionMonitorBase
{
    /// <summary>
    /// The socket to monitor for connection health
    /// </summary>
    private readonly ISendingReceivingSocket _socket;

    /// <summary>
    /// Configuration options for the connection monitor
    /// </summary>
    private readonly ConnectionMonitorOptions _options;

    /// <summary>
    /// Stopwatch to track time since last ping received
    /// </summary>
    private readonly Stopwatch _stopwatch = new();

    /// <summary>
    /// Timer for sending periodic ping frames
    /// </summary>
    private ISequentialTimer? _timer;

    /// <summary>
    /// Initializes a new instance of the DefaultConnectionMonitor class
    /// </summary>
    /// <param name="socket">The socket to monitor for connection health</param>
    /// <param name="options">Configuration options for the monitor</param>
    /// <param name="logger">Logger for tracing monitor operations</param>
    public DefaultConnectionMonitor(ISendingReceivingSocket socket, ConnectionMonitorOptions options, ILogger logger)
        : base(logger)
    {
        _socket = socket;
        _options = options;
        this.Trace("options: {options}", options);
    }

    /// <summary>
    /// Handles the start of connection monitoring by subscribing to socket events and starting the ping timer
    /// </summary>
    protected override void HandleStart()
    {
        this.Trace("start");

        this.Trace("subscribe to socket messages");
        _socket.OnReceived += HandleOnReceived;

        this.Trace("start timer");
        _timer = Timers.Async(HandlePingPongAsync, _options.PingInterval, _options.PingInterval, Logger);

        this.Trace("start stopwatch");
        _stopwatch.Restart();

        this.Trace("done");
    }

    /// <summary>
    /// Handles the stop of connection monitoring by unsubscribing from events and disposing resources
    /// </summary>
    protected override void HandleStop()
    {
        this.Trace("start");

        this.Trace("unsubscribe from socket messages");
        _socket.OnReceived -= HandleOnReceived;

        this.Trace("dispose timer");
        _timer.NotNull().Dispose();

        this.Trace("stop stopwatch");
        _stopwatch.Stop();

        this.Trace("done");
    }

    /// <summary>
    /// Handles the periodic ping-pong cycle by sending ping frames and checking for connection timeout
    /// </summary>
    /// <returns>A task representing the asynchronous ping-pong operation</returns>
    private async ValueTask HandlePingPongAsync()
    {
        this.Trace("start");

        var pingDelay = _stopwatch.ElapsedMilliseconds;

        this.Trace("send ping");
        await _socket.SendAsync(ProtocolFrames.Ping);

        if (IsRunning == 0)
        {
            this.Trace("skip - already stopped");
            return;
        }

        this.Trace("check last ping time");
        if (pingDelay > _options.MaxPingDelay)
        {
            this.Trace(
                "connection lost - {delay}ms without ping with max of {max}ms",
                pingDelay,
                _options.MaxPingDelay
            );
            FireConnectionLost();
        }

        this.Trace("done");
    }

    /// <summary>
    /// Handles received data from the socket and resets the ping timer if a ping frame is received
    /// </summary>
    /// <param name="data">The data received from the socket</param>
    private void HandleOnReceived(ReadOnlyMemory<byte> data)
    {
        if (!data.Span.SequenceEqual(ProtocolFrames.Ping.Span))
            return;

        if (IsRunning == 0)
        {
            this.Trace("skip - already stopped");
            return;
        }

        this.Trace("handle ping");
        _stopwatch.Restart();
    }
}
