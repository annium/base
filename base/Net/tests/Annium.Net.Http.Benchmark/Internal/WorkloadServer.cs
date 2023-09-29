using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Base;
using Annium.Net.Servers;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Net.Http.Benchmark.Internal;

internal static class WorkloadServer
{
    public static async Task RunAsync(CancellationToken ct)
    {
        var sp = new ServiceCollection().BuildServiceProvider();
        var server = WebServerBuilder.New(sp, Constants.Port).WithHttp(HandleHttpRequest).Build();
        await server.RunAsync(ct);
    }

    private static Task HandleHttpRequest(IServiceProvider sp, HttpListenerContext ctx, CancellationToken ct)
    {
        var path = ctx.Request.Url.NotNull().AbsolutePath;

        return path switch
        {
            "/params"   => HandleHttpParamsRequest(ctx),
            "/upload"   => HandleHttpUploadRequest(ctx),
            "/download" => HandleHttpDownloadRequest(ctx),
            _           => Task.CompletedTask
        };
    }

    private static Task HandleHttpParamsRequest(HttpListenerContext ctx)
    {
        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        ctx.Response.Close();

        return Task.CompletedTask;
    }

    private static Task HandleHttpUploadRequest(HttpListenerContext ctx)
    {
        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        ctx.Response.Close();

        return Task.CompletedTask;
    }

    private static async Task HandleHttpDownloadRequest(HttpListenerContext ctx)
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