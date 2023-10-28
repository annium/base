using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Xunit.Abstractions;

namespace Annium.Net.Http.Tests;

public abstract class TestBase : Testing.TestBase
{
    private static int _basePort = 14000;
    protected readonly Uri ServerUri;
    private readonly int _port;

    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _port = Interlocked.Increment(ref _basePort);
        ServerUri = new Uri($"http://127.0.0.1:{_port}");
    }

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
            cts.Cancel();
            await serverTask;
        });
    }
}

file class HttpHandler : IHttpHandler
{
    private readonly Func<HttpListenerContext, Task> _handle;

    public HttpHandler(Func<HttpListenerContext, Task> handle)
    {
        _handle = handle;
    }

    public Task HandleAsync(HttpListenerContext socket, CancellationToken ct)
    {
        return _handle(socket);
    }
}
