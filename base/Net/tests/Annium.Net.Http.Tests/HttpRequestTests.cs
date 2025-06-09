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

namespace Annium.Net.Http.Tests;

/// <summary>
/// Test class for HTTP request functionality.
/// </summary>
public class HttpRequestTests : TestBase
{
    /// <summary>
    /// The HTTP request factory for creating requests.
    /// </summary>
    private readonly IHttpRequestFactory _httpRequestFactory;

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

        // act
        this.Trace("send text");
        var response = await _httpRequestFactory
            .New(ServerUri)
            .Get("/")
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        response.IsAbort.IsTrue();
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
        await using var _ = RunServer(
            (_, response) =>
            {
                response.Ok();

                return Task.CompletedTask;
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri).Get("/").RunAsync(new CancellationToken(true));

        // assert
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
        await using var _ = RunServer(
            async (_, response) =>
            {
                await Task.Delay(100);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(ServerUri)
            .Get("/")
            .Timeout(TimeSpan.FromMilliseconds(50))
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
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
        await using var _ = RunServer(
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
            .New(ServerUri)
            .With(HttpMethod.Patch, "/")
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
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
        await using var _ = RunServer(
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
            .New(ServerUri)
            .Head("/")
            .Header(headerKey, headerValue)
            .RunAsync(TestContext.Current.CancellationToken);

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

    /// <summary>
    /// Tests that sending request with query parameters works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Send_Params()
    {
        this.Trace("start");

        // arrange
        await using var _ = RunServer(
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
            .New(ServerUri)
            .Get("/")
            .Param("x", "a")
            .Param("y", new[] { "b", "c" })
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
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
        await using var _ = RunServer(
            async (request, response) =>
            {
                await request.InputStream.CopyToAsync(response.OutputStream);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(ServerUri)
            .Post("/")
            .StringContent(message)
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        response.IsAbort.IsFalse();
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        responseContent.Is(message);

        this.Trace("done");
    }
}
