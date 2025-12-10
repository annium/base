using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Annium.Net.WebSockets.Internal;

namespace Annium.Net.WebSockets.Benchmark.Internal;

/// <summary>
/// Server that generates workload for WebSocket benchmarks
/// </summary>
internal static class WorkloadServer
{
    /// <summary>
    /// Workload server Uri
    /// </summary>
    public static Uri Uri => _server.WebSocketsUri();

    /// <summary>
    /// Underlying server instance that powers the benchmark workload.
    /// </summary>
    private static readonly IServer _server;

    /// <summary>
    /// Creates and starts workload server
    /// </summary>
    static WorkloadServer()
    {
        var services = new ServiceContainer();
        services.Add<WebSocketHandler>().AsSelf().Singleton();
        services.Add(VoidLogger.Instance).AsSelf().Singleton();
        var sp = services.BuildServiceProvider();

        _server = ServerBuilder.New(sp).WithWebSocketHandler<WebSocketHandler>().Start().NotNull();
    }

    /// <summary>
    /// Starts the workload server by triggering initialization.
    /// </summary>
    public static void Start()
    {
        // NOOP, needed to trigger static constructor
    }

    /// <summary>
    /// Stops the workload server and disposes underlying resources.
    /// </summary>
    /// <returns>A task that completes when the server shuts down.</returns>
    public static async Task StopAsync()
    {
        await _server.DisposeAsync();
    }
}

/// <summary>
/// WebSocket handler that sends workload messages to connected clients
/// </summary>
file class WebSocketHandler : IWebSocketHandler
{
    /// <summary>
    /// Template message for workload generation
    /// </summary>
    private const string WorkloadMessage =
        "{{\"stream\":\"{0}@aggTrade\",\"data\":{{\"e\":\"aggTrade\",\"E\":1689659049498,\"s\":\"{1}\",\"a\":2675370021,\"p\":\"30048.53000000\",\"q\":\"0.00332000\",\"f\":3174123265,\"l\":3174123265,\"T\":1689659049497,\"m\":false,\"M\":true}}}}";

    /// <summary>
    /// Logger instance for the handler
    /// </summary>
    private readonly ILogger _logger;

    public WebSocketHandler(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles WebSocket connections and sends workload messages
    /// </summary>
    /// <param name="ctx">WebSocket context</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Task representing the handling operation</returns>
    public async Task HandleAsync(HttpListenerWebSocketContext ctx, CancellationToken ct)
    {
        var clientSocket = new ManagedWebSocket(ctx.WebSocket, _logger);

        ReadOnlyMemory<byte> workloadMessageBytes = Encoding.UTF8.GetBytes(WorkloadMessage).AsMemory();
        for (var i = 0; i < Constants.TotalMessages; i++)
            await clientSocket.SendTextAsync(workloadMessageBytes, CancellationToken.None).ConfigureAwait(false);

        await ctx.WebSocket.CloseOutputAsync(
            System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
            null,
            CancellationToken.None
        );
    }
}
