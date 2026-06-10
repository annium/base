using System;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Servers.Web.Tests;

/// <summary>
/// Tests for HTTP and WebSocket request routing based on which handlers are registered.
/// </summary>
public class HandlerRoutingTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the HandlerRoutingTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public HandlerRoutingTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// When only an HTTP handler is registered a WebSocket upgrade attempt must not establish
    /// a connection.  The server has no WebSocket handler, so CloseConnectionAsync returns 404,
    /// causing the client ConnectAsync to fail (the server rejected the upgrade).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HttpOnlyServer_WebSocketUpgrade_DoesNotEstablish()
    {
        this.Trace("start");

        // arrange — HTTP-only server; no WebSocket handler
        var server = RunHttpServer();
        var wsUri = server.WebSocketsUri();
        this.Trace<Uri>("ws uri: {wsUri}", wsUri);

        // act
        using var wsClient = new ClientWebSocket();
        var connectTask = wsClient.ConnectAsync(wsUri, TestContext.Current.CancellationToken);

        // assert — the upgrade is rejected so ConnectAsync must throw
        await Wrap.It(async () =>
                await connectTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<WebSocketException>();

        // state is never Open
        wsClient.State.IsNot(WebSocketState.Open);

        await server.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        this.Trace("done");
    }

    /// <summary>
    /// When only a WebSocket handler is registered a plain HTTP GET must receive 404.
    /// The server has no HTTP handler, so CloseConnectionAsync is invoked for every HTTP request.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WebSocketOnlyServer_HttpRequest_Returns404()
    {
        this.Trace("start");

        // arrange — WebSocket-only server; no HTTP handler
        var server = RunServer(
            wsHandle: async (ctx, ct) =>
            {
                var buf = new byte[1024];
                var result = await ctx.WebSocket.ReceiveAsync(buf, ct);
                await ctx.WebSocket.SendAsync(
                    new ArraySegment<byte>(buf, 0, result.Count),
                    WebSocketMessageType.Text,
                    true,
                    ct
                );
                await ctx.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", ct);
            }
        );

        // act
        using var httpClient = new HttpClient();
        var response = await httpClient
            .GetAsync(server.HttpUri(), TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.NotFound);

        await server.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        this.Trace("done");
    }

    /// <summary>
    /// Happy-path: HTTP handler writes a specific body; the client receives exactly that body
    /// and a 200 status code.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HttpHandler_ReturnsExpectedResponse()
    {
        this.Trace("start");

        // arrange
        const string expected = "hello from server";
        var server = RunHttpServer(expected);

        // act
        using var httpClient = new HttpClient();
        var response = await httpClient
            .GetAsync(server.HttpUri(), TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // assert
        response.StatusCode.Is(HttpStatusCode.OK);
        body.Is(expected);

        await server.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        this.Trace("done");
    }

    /// <summary>
    /// Happy-path WebSocket: handler echoes a text message; the client receives the same text.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task WebSocketHandler_EchoesTextMessage()
    {
        this.Trace("start");

        // arrange — echo server
        var server = RunServer(
            wsHandle: async (ctx, ct) =>
            {
                var buf = new byte[4096];
                var result = await ctx.WebSocket.ReceiveAsync(buf, ct);
                await ctx.WebSocket.SendAsync(
                    new ArraySegment<byte>(buf, 0, result.Count),
                    WebSocketMessageType.Text,
                    true,
                    ct
                );
                await ctx.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", ct);
            }
        );

        const string message = "ping";
        using var wsClient = new ClientWebSocket();
        await wsClient
            .ConnectAsync(server.WebSocketsUri(), TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        wsClient.State.Is(WebSocketState.Open);

        // act
        var sendBuf = Encoding.UTF8.GetBytes(message);
        await wsClient.SendAsync(
            new ArraySegment<byte>(sendBuf),
            WebSocketMessageType.Text,
            true,
            TestContext.Current.CancellationToken
        );

        var recvBuf = new byte[4096];
        var recvResult = await wsClient
            .ReceiveAsync(recvBuf, TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // assert
        recvResult.MessageType.Is(WebSocketMessageType.Text);
        var received = Encoding.UTF8.GetString(recvBuf, 0, recvResult.Count);
        received.Is(message);

        // clean client-side close
        await wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", TestContext.Current.CancellationToken);

        await server.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        this.Trace("done");
    }

    /// <summary>
    /// Both handlers registered: HTTP request goes to the HTTP handler (200), and a
    /// WebSocket upgrade goes to the WebSocket handler (connection established).
    /// This validates the request-type routing branch in Server.HandleRequest.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DualHandlerServer_RoutesHttpAndWebSocket_Correctly()
    {
        this.Trace("start");

        // arrange — dual-handler server; WS handler echoes one message then waits for client-initiated close
        const string httpResponse = "http-ok";
        const string wsMessage = "dual-test";
        var server = RunServer(
            httpHandle: async (ctx, ct) =>
            {
                var data = Encoding.UTF8.GetBytes(httpResponse);
                ctx.Response.StatusCode = 200;
                await ctx.Response.OutputStream.WriteAsync(data, ct);
                ctx.Response.Close();
            },
            wsHandle: async (ctx, ct) =>
            {
                // echo one message, then the client drives the close
                var buf = new byte[4096];
                var result = await ctx.WebSocket.ReceiveAsync(buf, ct);
                await ctx.WebSocket.SendAsync(
                    new ArraySegment<byte>(buf, 0, result.Count),
                    WebSocketMessageType.Text,
                    true,
                    ct
                );
                // wait for client to initiate close
                await ctx.WebSocket.ReceiveAsync(buf, ct);
                await ctx.WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "done", ct);
            }
        );

        // act: HTTP
        using var httpClient = new HttpClient();
        var httpResp = await httpClient
            .GetAsync(server.HttpUri(), TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        var httpBody = await httpResp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // assert: HTTP
        httpResp.StatusCode.Is(HttpStatusCode.OK);
        httpBody.Is(httpResponse);

        // act: WebSocket — connect, send, receive echo, then close
        using var wsClient = new ClientWebSocket();
        await wsClient
            .ConnectAsync(server.WebSocketsUri(), TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        wsClient.State.Is(WebSocketState.Open);

        var sendBuf = Encoding.UTF8.GetBytes(wsMessage);
        await wsClient.SendAsync(
            new ArraySegment<byte>(sendBuf),
            WebSocketMessageType.Text,
            true,
            TestContext.Current.CancellationToken
        );

        var recvBuf = new byte[4096];
        var echoResult = await wsClient
            .ReceiveAsync(recvBuf, TestContext.Current.CancellationToken)
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        echoResult.MessageType.Is(WebSocketMessageType.Text);
        Encoding.UTF8.GetString(recvBuf, 0, echoResult.Count).Is(wsMessage);

        // client initiates close
        await wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", TestContext.Current.CancellationToken);

        await server.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        this.Trace("done");
    }
}
