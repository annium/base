using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Http.Tests;

/// <summary>
/// Test class for HTTP request functionality.
/// </summary>
public class HttpRequestTests : TestBase
{
    /// <summary>
    /// The HTTP request factory for creating requests.
    /// </summary>
    private IHttpRequestFactory _httpRequestFactory = null!; // set in InitializeAsync

    /// <summary>
    /// Initializes a new instance of the HttpRequestTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public HttpRequestTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(true);
            container.AddHttpRequestFactory(true);
        });
    }

    /// <summary>
    /// Runs base initialization and resolves the <see cref="IHttpRequestFactory"/> from the DI container.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization.</returns>
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _httpRequestFactory = Get<IHttpRequestFactory>();
    }

    /// <summary>
    /// Tests that sending request to non-connected server returns proper abort status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Send_NotConnected()
    {
        this.Trace("start");

        // arrange — target the IANA-assigned "discard" port on loopback (RFC 863). The
        // service it describes exists only by convention; it is not started by any modern
        // OS out of the box and requires root to bind on Linux, so connect attempts get
        // an immediate ECONNREFUSED on every platform without any bind/race dance.
        var uri = new Uri("http://127.0.0.1:9/");

        // act
        this.Trace("send to discard port {uri}", uri);
        var response = await _httpRequestFactory.New(uri).Get("/").RunAsync(TestContext.Current.CancellationToken);

        // assert
        response.IsNetworkError.IsTrue();
        response.IsAbort.IsFalse();
        response.IsSuccess.IsFalse();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.ServiceUnavailable);

        this.Trace("done");
    }

    /// <summary>
    /// Tests that sending request with cancelled token returns proper abort status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Send_Canceled()
    {
        this.Trace("start");

        // arrange
        await using var server = RunServer(
            (_, response) =>
            {
                response.Ok();

                return Task.CompletedTask;
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(server.HttpUri()).Get("/").RunAsync(new CancellationToken(true));

        // assert
        response.IsNetworkError.IsFalse();
        response.IsAbort.IsTrue();
        response.IsSuccess.IsFalse();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.RequestTimeout);

        this.Trace("done");
    }

    /// <summary>
    /// Tests that sending request with timeout returns proper abort status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Send_Timeout()
    {
        this.Trace("start");

        // arrange
        await using var server = RunServer(
            async (_, response) =>
            {
                await Task.Delay(100);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(server.HttpUri())
            .Get("/")
            .Timeout(TimeSpan.FromMilliseconds(50))
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        response.IsNetworkError.IsFalse();
        response.IsAbort.IsTrue();
        response.IsSuccess.IsFalse();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.RequestTimeout);

        this.Trace("done");
    }

    /// <summary>
    /// Tests that sending request with custom HTTP method works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Send_CustomMethod()
    {
        this.Trace("start");

        // arrange
        await using var server = RunServer(
            async (request, response) =>
            {
                var data = Encoding.UTF8.GetBytes(request.HttpMethod);
                await response.OutputStream.WriteAsync(data);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(server.HttpUri())
            .With(HttpMethod.Patch, "/")
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        response.IsNetworkError.IsFalse();
        response.IsAbort.IsFalse();
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        responseContent.Is(HttpMethod.Patch.ToString());

        this.Trace("done");
    }

    /// <summary>
    /// Tests that sending request with custom headers works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Send_Headers()
    {
        this.Trace("start");

        // arrange
        const string headerPrefix = "custom";
        const string headerKey = $"{headerPrefix}-header";
        const string headerValue = $"{headerPrefix} content";
        await using var server = RunServer(
            (request, response) =>
            {
                var targetHeaders = request
                    .Headers.AllKeys.OfType<string>()
                    .Where(x => x.StartsWith(headerPrefix))
                    .ToArray();

                foreach (var key in targetHeaders)
                    response.Headers.Add(key, request.Headers.Get(key));

                response.Ok();

                return Task.CompletedTask;
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(server.HttpUri())
            .Head("/")
            .Header(headerKey, headerValue)
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        response.IsNetworkError.IsFalse();
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

    /// <summary>
    /// Tests that sending request with query parameters works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Send_Params()
    {
        this.Trace("start");

        // arrange
        await using var server = RunServer(
            async (request, response) =>
            {
                var data = Encoding.UTF8.GetBytes(request.Url.NotNull().Query);
                await response.OutputStream.WriteAsync(data);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(server.HttpUri())
            .Get("/")
            .Param("x", "a")
            .Param("y", new[] { "b", "c" })
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        response.IsNetworkError.IsFalse();
        response.IsAbort.IsFalse();
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        responseContent.Is("?x=a&y=b&y=c");

        this.Trace("done");
    }

    /// <summary>
    /// Tests that sending request with content body works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Send_Content()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";
        await using var server = RunServer(
            async (request, response) =>
            {
                await request.InputStream.CopyToAsync(response.OutputStream);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(server.HttpUri())
            .Post("/")
            .StringContent(message)
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        response.IsNetworkError.IsFalse();
        response.IsAbort.IsFalse();
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        responseContent.Is(message);

        this.Trace("done");
    }

    /// <summary>
    /// A union reads the failure shape even when the success shape throws while being read.
    /// </summary>
    /// <remarks>
    /// The two shapes are alternatives, and failing to be one is not a reason to stop asking about the
    /// other. They used to be parsed inside one try, so a throw on the success attempt abandoned the
    /// failure attempt with it - and a body that is an error object, against a success type that is a
    /// collection, throws every time. The caller was told the response could not be parsed while the
    /// server had said plainly what was wrong.
    /// </remarks>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsResponse_Union_ReadsTheFailureWhenTheSuccessShapeThrows()
    {
        this.Trace("start");

        // arrange - an error object, against a success type that can only be an array
        await using var server = RunServer(
            async (_, response) =>
            {
                var payload = Encoding.UTF8.GetBytes(@"{""code"":-2015,""msg"":""Invalid API-key.""}");
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.ContentType = "application/json";
                response.ContentLength64 = payload.Length;
                await response.OutputStream.WriteAsync(payload);
            }
        );

        // act
        var response = await _httpRequestFactory
            .New(server.HttpUri())
            .Get("/")
            .AsResponseAsync<int[], Failure>(TestContext.Current.CancellationToken);

        // assert
        response.Data.IsT1.IsTrue("the failure shape must be read even though the success shape threw");
        response.Data.AsT1.NotNull().Code.Is(-2015);
        response.Data.AsT1.NotNull().Msg.Is("Invalid API-key.");

        this.Trace("done");
    }

    /// <summary>
    /// A body that is neither shape still reports as neither, rather than being forced into one.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsResponse_Union_ReadsNeitherWhenTheBodyIsNeither()
    {
        this.Trace("start");

        // arrange
        await using var server = RunServer(
            async (_, response) =>
            {
                var payload = Encoding.UTF8.GetBytes(@"""just a string""");
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.ContentType = "application/json";
                response.ContentLength64 = payload.Length;
                await response.OutputStream.WriteAsync(payload);
            }
        );

        // act
        var response = await _httpRequestFactory
            .New(server.HttpUri())
            .Get("/")
            .AsResponseAsync<int[], Failure>(TestContext.Current.CancellationToken);

        // assert
        response.Data.IsT1.IsTrue();
        response.Data.AsT1.IsDefault("a body matching neither shape must not be presented as either");

        this.Trace("done");
    }

    /// <summary>An error payload shaped the way many APIs report one.</summary>
    /// <param name="Code">The provider's own error code.</param>
    /// <param name="Msg">The provider's own error message.</param>
    private sealed record Failure(int Code, string Msg);
}
