using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Base;
using Annium.Net.Servers.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Net.Http.Benchmark.Internal;

/// <summary>
/// Provides a workload server for HTTP benchmarks.
/// </summary>
internal static class WorkloadServer
{
    /// <summary>
    /// Runs the workload server asynchronously.
    /// </summary>
    /// <param name="ct">Cancellation token to stop the server.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunAsync(CancellationToken ct)
    {
        var services = new ServiceCollection();
        services.AddSingleton<HttpHandler>();
        var sp = services.BuildServiceProvider();
        var server = ServerBuilder.New(sp, Constants.Port).WithHttpHandler<HttpHandler>().Build();
        await server.RunAsync(ct);
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
