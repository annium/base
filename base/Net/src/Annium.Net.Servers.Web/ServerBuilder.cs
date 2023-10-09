using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Servers.Web.Internal;

namespace Annium.Net.Servers.Web;

public static class ServerBuilder
{
    public static IServerBuilder New(IServiceProvider sp, int port)
    {
        return new ServerBuilderInstance(sp, port);
    }
}

public interface IServerBuilder
{
    IServerBuilder WithHttp(Func<IServiceProvider, HttpListenerContext, CancellationToken, Task> handler);
    IServerBuilder WithWebSockets(Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> handler);
    IServer Build();
}

file class ServerBuilderInstance : IServerBuilder
{
    private readonly IServiceProvider _sp;
    private readonly int _port;
    private Func<IServiceProvider, HttpListenerContext, CancellationToken, Task>? _handleHttp;
    private Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task>? _handleWebSocket;

    public ServerBuilderInstance(IServiceProvider sp, int port)
    {
        _sp = sp;
        _port = port;
    }

    public IServerBuilder WithHttp(Func<IServiceProvider, HttpListenerContext, CancellationToken, Task> handler)
    {
        _handleHttp = handler;

        return this;
    }

    public IServerBuilder WithWebSockets(Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> handler)
    {
        _handleWebSocket = handler;

        return this;
    }

    public IServer Build()
    {
        return new Server(_sp, _port, _handleHttp, _handleWebSocket);
    }
}