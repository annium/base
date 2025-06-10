using System.Net;
using System.Text;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Http.Extensions;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Http.Tests;

/// <summary>
/// Test class for HTTP request configuration and interception functionality.
/// </summary>
public class HttpRequestConfigureInterceptTests : TestBase
{
    /// <summary>
    /// The HTTP request factory for creating requests.
    /// </summary>
    private readonly IHttpRequestFactory _httpRequestFactory;

    /// <summary>
    /// Initializes a new instance of the HttpRequestConfigureInterceptTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public HttpRequestConfigureInterceptTests(ITestOutputHelper outputHelper)
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
    /// Tests that HTTP request configuration works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Configure()
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
            .Configure(req => req.Param("x", "a"))
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        responseContent.Is("?x=a");

        this.Trace("done");
    }

    /// <summary>
    /// Tests that HTTP request interception with next() function works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Intercept_Next()
    {
        this.Trace("start");

        // arrange
        var message = "demo";
        var log = new TestLog<string>();
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
            .Get("/")
            .Intercept(async next =>
            {
                log.Add("before");
                var response = await next();
                log.Add("after");

                return response;
            })
            .StringContent(message)
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        responseContent.Is(message);
        log.IsEqual(new[] { "before", "after" });

        this.Trace("done");
    }

    /// <summary>
    /// Tests that HTTP request interception with access to request object works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Intercept_Next_Request()
    {
        this.Trace("start");

        // arrange
        var message = "demo";
        var log = new TestLog<string>();
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
            .Get("/")
            .Intercept(
                async (next, req) =>
                {
                    log.Add($"before {req.Params["x"]}");
                    var response = await next();
                    log.Add($"after {req.Params["x"]}");

                    return response;
                }
            )
            .Param("x", 1)
            .StringContent(message)
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        response.IsSuccess.IsTrue();
        response.IsFailure.IsFalse();
        response.StatusCode.Is(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        responseContent.Is(message);
        log.IsEqual(new[] { "before 1", "after 1" });

        this.Trace("done");
    }
}
