using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Base;
using Annium.Net.Servers.Web;

namespace Annium.Net.Http.Benchmark.Internal;

/// <summary>
/// Provides a workload server for HTTP benchmarks.
/// </summary>
internal static class WorkloadServer
{
    /// <summary>
    /// Workload server Uri
    /// </summary>
    public static Uri Uri => _server.HttpUri();

    /// <summary>
    /// Underlying server instance used for benchmark requests.
    /// </summary>
    private static readonly IServer _server;

    /// <summary>
    /// Creates and starts workload server
    /// </summary>
    static WorkloadServer()
    {
        var services = new ServiceContainer();
        services.Add<HttpHandler>().AsSelf().Singleton();
        services.Add(VoidLogger.Instance).AsSelf().Singleton();
        var sp = services.BuildServiceProvider();

        _server = ServerBuilder.New(sp).WithHttpHandler<HttpHandler>().Start().NotNull();
    }

    /// <summary>
    /// Starts the workload server by triggering the static constructor.
    /// </summary>
    public static void Start()
    {
        // NOOP, needed to trigger static constructor
    }

    /// <summary>
    /// Stops the workload server and releases resources.
    /// </summary>
    /// <returns>A task that completes when the server is disposed.</returns>
    public static async Task StopAsync()
    {
        await _server.DisposeAsync();
    }
}

/// <summary>
/// HTTP handler for benchmark workload requests.
/// </summary>
file class HttpHandler : IHttpHandler
{
    /// <summary>
    /// Handles incoming HTTP requests.
    /// </summary>
    /// <param name="ctx">The HTTP listener context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task HandleAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        var path = ctx.Request.Url.NotNull().AbsolutePath;

        return path switch
        {
            "/params" => HandleHttpParamsRequestAsync(ctx),
            "/upload" => HandleHttpUploadRequestAsync(ctx),
            "/download" => HandleHttpDownloadRequestAsync(ctx),
            _ => Task.CompletedTask,
        };
    }

    /// <summary>
    /// Handles HTTP requests to the params endpoint.
    /// </summary>
    /// <param name="ctx">The HTTP listener context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task HandleHttpParamsRequestAsync(HttpListenerContext ctx)
    {
        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        ctx.Response.Close();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles HTTP requests to the upload endpoint.
    /// </summary>
    /// <param name="ctx">The HTTP listener context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static Task HandleHttpUploadRequestAsync(HttpListenerContext ctx)
    {
        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        ctx.Response.Close();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles HTTP requests to the download endpoint.
    /// </summary>
    /// <param name="ctx">The HTTP listener context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task HandleHttpDownloadRequestAsync(HttpListenerContext ctx)
    {
        var query = UriQuery.Parse(ctx.Request.Url.NotNull().Query);
        var size = int.Parse(query["size"]!, CultureInfo.InvariantCulture);
        var content = Helper.GetContent(size);
        ctx.Response.Headers.Clear();
        ctx.Response.SendChunked = false;
        await ctx.Response.OutputStream.WriteAsync(content, CancellationToken.None);
        ctx.Response.OutputStream.Close();
        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        ctx.Response.Close();
    }
}
