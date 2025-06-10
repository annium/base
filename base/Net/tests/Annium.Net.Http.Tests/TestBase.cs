using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Xunit;

namespace Annium.Net.Http.Tests;

/// <summary>
/// Base class for HTTP tests providing server setup functionality.
/// </summary>
public abstract class TestBase : Testing.TestBase
{
    /// <summary>
    /// Base port number for test servers.
    /// </summary>
    private static int _basePort = 40000;

    /// <summary>
    /// The server URI for the test server.
    /// </summary>
    protected readonly Uri ServerUri;

    /// <summary>
    /// The port number used by this test instance.
    /// </summary>
    private readonly int _port;

    /// <summary>
    /// Initializes a new instance of the TestBase class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _port = Interlocked.Increment(ref _basePort);
        ServerUri = new Uri($"http://127.0.0.1:{_port}");
    }

    /// <summary>
    /// Runs a test server with the specified request handler.
    /// </summary>
    /// <param name="handle">The function to handle HTTP requests.</param>
    /// <returns>An IAsyncDisposable to stop the server.</returns>
    protected IAsyncDisposable RunServer(Func<HttpListenerRequest, HttpListenerResponse, Task> handle)
    {
        var handler = new HttpHandler(async ctx =>
        {
            this.Trace("start");

            ctx.Response.Headers.Clear();
            try
            {
                await handle(ctx.Request, ctx.Response);
            }
            catch
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                ctx.Response.Close();
            }

            this.Trace("done");
        });

        var server = ServerBuilder.New(Get<IServiceProvider>(), _port).WithHttpHandler(handler).Build();
        var cts = new CancellationTokenSource();
        var serverTask = server.RunAsync(cts.Token);

        return Disposable.Create(async () =>
        {
            // await before cancellation for a while
            await Task.Delay(5, CancellationToken.None);
            await cts.CancelAsync();
#pragma warning disable VSTHRD003
            await serverTask;
#pragma warning restore VSTHRD003
        });
    }
}

/// <summary>
/// HTTP handler implementation for test servers.
/// </summary>
file class HttpHandler : IHttpHandler
{
    /// <summary>
    /// The function to handle HTTP requests.
    /// </summary>
    private readonly Func<HttpListenerContext, Task> _handle;

    /// <summary>
    /// Initializes a new instance of the HttpHandler class.
    /// </summary>
    /// <param name="handle">The function to handle HTTP requests.</param>
    public HttpHandler(Func<HttpListenerContext, Task> handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Handles an incoming HTTP request.
    /// </summary>
    /// <param name="socket">The HTTP listener context.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task HandleAsync(HttpListenerContext socket, CancellationToken ct)
    {
        return _handle(socket);
    }
}
