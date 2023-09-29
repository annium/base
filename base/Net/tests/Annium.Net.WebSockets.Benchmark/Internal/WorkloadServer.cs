using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Servers;
using Annium.Net.WebSockets.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Net.WebSockets.Benchmark.Internal;

internal static class WorkloadServer
{
    const string WorkloadMessage = "{{\"stream\":\"{0}@aggTrade\",\"data\":{{\"e\":\"aggTrade\",\"E\":1689659049498,\"s\":\"{1}\",\"a\":2675370021,\"p\":\"30048.53000000\",\"q\":\"0.00332000\",\"f\":3174123265,\"l\":3174123265,\"T\":1689659049497,\"m\":false,\"M\":true}}}}";

    public static async Task RunAsync(CancellationToken ct)
    {
        var sp = new ServiceCollection().BuildServiceProvider();
        var server = WebServerBuilder.New(sp, Constants.Port).WithWebSockets(HandleWebSocket).Build();
        await server.RunAsync(ct);
    }

    private static async Task HandleWebSocket(IServiceProvider sp, HttpListenerWebSocketContext ctx, CancellationToken ct)
    {
        var clientSocket = new ManagedWebSocket(ctx.WebSocket, sp.Resolve<ILogger>());

        ReadOnlyMemory<byte> workloadMessageBytes = Encoding.UTF8.GetBytes(WorkloadMessage).AsMemory();
        for (var i = 0; i < Constants.TotalMessages; i++)
            await clientSocket.SendTextAsync(workloadMessageBytes, CancellationToken.None).ConfigureAwait(false);

        await ctx.WebSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
    }
}