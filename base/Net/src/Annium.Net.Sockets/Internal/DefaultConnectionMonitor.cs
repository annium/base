using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Threading;

namespace Annium.Net.Sockets.Internal;

internal class DefaultConnectionMonitor : ConnectionMonitorBase
{
    private readonly ConnectionMonitorOptions _options;
    private readonly ISendingReceivingSocket _socket;
    private readonly Stopwatch _stopwatch = new();
    private AsyncTimer? _timer;

    public DefaultConnectionMonitor(ConnectionMonitorOptions options, ISendingReceivingSocket socket, ILogger logger)
        : base(logger)
    {
        _options = options;
        _socket = socket;
    }

    protected override void HandleStart()
    {
        this.Trace("start");

        this.Trace("subscribe to socket messages");
        _socket.OnReceived += HandleOnReceived;

        this.Trace("start timer");
        _timer = AsyncTimer.Create(HandlePingPong, _options.PingInterval, _options.PingInterval);

        this.Trace("start stopwatch");
        _stopwatch.Restart();

        this.Trace("done");
    }

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

    private async ValueTask HandlePingPong()
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
