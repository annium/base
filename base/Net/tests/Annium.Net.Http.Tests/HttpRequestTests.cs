using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Http.Tests;

public class HttpRequestTests : TestBase
{
    private readonly IHttpRequestFactory _httpRequestFactory;

    public HttpRequestTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(true);
            container.AddHttpRequestFactory(true);
        });
        _httpRequestFactory = Get<IHttpRequestFactory>();
    }

    [Fact]
    public async Task Send_NotConnected()
    {
        this.Trace("start");

        // act
        this.Trace("send text");
        var response = await _httpRequestFactory.New(ServerUri).Get("/").RunAsync();

        // assert
        response.IsAbort.IsTrue();
        response.IsSuccess.IsFalse();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.ServiceUnavailable);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Canceled()
    {
        this.Trace("start");

        // arrange
        await using var _ = RunServer((_, response) =>
        {
            response.Ok();

            return Task.CompletedTask;
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .Get("/")
            .RunAsync(new CancellationToken(true));

        // assert
        response.IsAbort.IsTrue();
        response.IsSuccess.IsFalse();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.RequestTimeout);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Timeout()
    {
        this.Trace("start");

        // arrange
        await using var _ = RunServer(async (_, response) =>
        {
            await Task.Delay(100);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .Get("/")
            .Timeout(TimeSpan.FromMilliseconds(50))
            .RunAsync();

        // assert
        response.IsAbort.IsTrue();
        response.IsSuccess.IsFalse();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.RequestTimeout);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_CustomMethod()
    {
        this.Trace("start");

        // arrange
        await using var _ = RunServer(async (request, response) =>
        {
            var data = Encoding.UTF8.GetBytes(request.HttpMethod);
            await response.OutputStream.WriteAsync(data);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .With(HttpMethod.Patch, "/")
            .RunAsync();

        // assert
        response.IsAbort.IsFalse();
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Is(HttpMethod.Patch.ToString());

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Headers()
    {
        this.Trace("start");

        // arrange
        const string headerPrefix = "custom";
        const string headerKey = $"{headerPrefix}-header";
        const string headerValue = $"{headerPrefix} content";
        await using var _ = RunServer((request, response) =>
        {
            var targetHeaders = request.Headers.AllKeys
                .OfType<string>()
                .Where(x => x.StartsWith(headerPrefix))
                .ToArray();

            foreach (var key in targetHeaders)
                response.Headers.Add(key, request.Headers.Get(key));

            response.Ok();

            return Task.CompletedTask;
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .Head("/")
            .Header(headerKey, headerValue)
            .RunAsync();

        // assert
        response.IsAbort.IsFalse();
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        response.Headers.TryGetValues(headerKey, out var headerValuesRaw).IsTrue();
        var headerValues = headerValuesRaw.NotNull().ToArray();
        headerValues.Has(1);
        headerValues.At(0).Is(headerValue);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Params()
    {
        this.Trace("start");

        // arrange
        await using var _ = RunServer(async (request, response) =>
        {
            var data = Encoding.UTF8.GetBytes(request.Url.NotNull().Query);
            await response.OutputStream.WriteAsync(data);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .Get("/")
            .Param("x", "a")
            .Param("y", new[] { "b", "c" })
            .RunAsync();

        // assert
        response.IsAbort.IsFalse();
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Is("?x=a&y=b&y=c");

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Content()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";
        await using var _ = RunServer(async (request, response) =>
        {
            await request.InputStream.CopyToAsync(response.OutputStream);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri)
            .Post("/")
            .StringContent(message)
            .RunAsync();

        // assert
        response.IsAbort.IsFalse();
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Is(message);

        this.Trace("done");
    }
}