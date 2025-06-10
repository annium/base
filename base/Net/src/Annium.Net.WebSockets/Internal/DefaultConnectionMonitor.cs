using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Threading;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Default implementation of connection monitoring using ping/pong frames
/// </summary>
internal class DefaultConnectionMonitor : ConnectionMonitorBase
{
    /// <summary>
    /// The WebSocket to monitor
    /// </summary>
    private readonly ISendingReceivingWebSocket _socket;

    /// <summary>
    /// Connection monitoring configuration options
    /// </summary>
    private readonly ConnectionMonitorOptions _options;

    /// <summary>
    /// Stopwatch for tracking ping timing
    /// </summary>
    private readonly Stopwatch _stopwatch = new();

    /// <summary>
    /// Timer for periodic ping operations
    /// </summary>
    private ISequentialTimer? _timer;

    /// <summary>
    /// Initializes a new instance of the DefaultConnectionMonitor class
    /// </summary>
    /// <param name="socket">The WebSocket to monitor</param>
    /// <param name="options">Configuration options for monitoring</param>
    /// <param name="logger">Logger instance for tracing and error reporting</param>
    public DefaultConnectionMonitor(ISendingReceivingWebSocket socket, ConnectionMonitorOptions options, ILogger logger)
        : base(logger)
    {
        _socket = socket;
        _options = options;
        this.Trace("options: {options}", options);
    }

    /// <summary>
    /// Handles the start of connection monitoring by subscribing to events and starting the ping timer
    /// </summary>
    protected override void HandleStart()
    {
        this.Trace("start");

        this.Trace("subscribe to socket messages");
        _socket.OnBinaryReceived += HandleOnReceived;

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
        _socket.OnBinaryReceived -= HandleOnReceived;

        this.Trace("dispose timer");
        _timer.NotNull().Dispose();

        this.Trace("stop stopwatch");
        _stopwatch.Stop();

        this.Trace("done");
    }

    /// <summary>
    /// Handles the periodic ping/pong operation to monitor connection health
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous ping operation</returns>
    private async ValueTask HandlePingPongAsync()
    {
        this.Trace("start");

        var pingDelay = _stopwatch.ElapsedMilliseconds;

        this.Trace("send ping");
        await _socket.SendBinaryAsync(ProtocolFrames.Ping);

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
    /// Handles received binary data to detect ping frames and reset the connection timer
    /// </summary>
    /// <param name="data">The received binary data to check for ping frames</param>
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
