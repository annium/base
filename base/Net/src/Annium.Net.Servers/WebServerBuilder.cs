using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Servers.Internal;

namespace Annium.Net.Servers;

public static class WebServerBuilder
{
    public static IWebServerBuilder New(IServiceProvider sp, int port)
    {
        return new WebServerBuilderInstance(sp, port);
    }
}

public interface IWebServerBuilder
{
    IWebServerBuilder WithHttp(Func<IServiceProvider, HttpListenerContext, CancellationToken, Task> handler);
    IWebServerBuilder WithWebSockets(Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> handler);
    IWebServer Build();
}

file class WebServerBuilderInstance : IWebServerBuilder
{
    private readonly IServiceProvider _sp;
    private readonly int _port;
    private Func<IServiceProvider, HttpListenerContext, CancellationToken, Task>? _handleHttp;
    private Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task>? _handleWebSocket;

    public WebServerBuilderInstance(IServiceProvider sp, int port)
    {
        _sp = sp;
        _port = port;
    }

    public IWebServerBuilder WithHttp(Func<IServiceProvider, HttpListenerContext, CancellationToken, Task> handler)
    {
        _handleHttp = handler;

        return this;
    }

    public IWebServerBuilder WithWebSockets(Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> handler)
    {
        _handleWebSocket = handler;

        return this;
    }

    public IWebServer Build()
    {
        return new WebServer(_sp, _port, _handleHttp, _handleWebSocket);
    }
}