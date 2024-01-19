using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Threading;

namespace Annium.Net.WebSockets.Internal;

internal class DefaultConnectionMonitor : ConnectionMonitorBase
{
    private readonly ISendingReceivingWebSocket _socket;
    private readonly ConnectionMonitorOptions _options;
    private readonly Stopwatch _stopwatch = new();
    private ISequentialTimer? _timer;

    public DefaultConnectionMonitor(ISendingReceivingWebSocket socket, ConnectionMonitorOptions options, ILogger logger)
        : base(logger)
    {
        _socket = socket;
        _options = options;
        this.Trace("options: {options}", options);
    }

    protected override void HandleStart()
    {
        this.Trace("start");

        this.Trace("subscribe to socket messages");
        _socket.OnBinaryReceived += HandleOnReceived;

        this.Trace("start timer");
        _timer = Timers.Async(HandlePingPong, _options.PingInterval, _options.PingInterval, Logger);

        this.Trace("start stopwatch");
        _stopwatch.Restart();

        this.Trace("done");
    }

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

    private async ValueTask HandlePingPong()
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
