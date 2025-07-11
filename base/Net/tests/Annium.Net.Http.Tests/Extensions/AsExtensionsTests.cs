using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Http.Internal;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Http.Tests.Extensions;

/// <summary>
/// Tests for HTTP request AsAsync extension methods that convert HTTP responses to strongly typed objects
/// </summary>
public class AsExtensionsTests : TestBase
{
    /// <summary>
    /// Factory for creating HTTP requests
    /// </summary>
    private readonly IHttpRequestFactory _httpRequestFactory;

    /// <summary>
    /// Serializer for JSON conversion operations
    /// </summary>
    private readonly Serializer _serializer;

    /// <summary>
    /// Initializes a new instance of the AsExtensionsTests class
    /// </summary>
    /// <param name="outputHelper">Test output helper for logging</param>
    public AsExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            Serialization
                .Abstractions.ServiceContainerExtensions.AddSerializers(container)
                .WithJson(
                    opts =>
                    {
                        opts.Converters.Clear();
                        opts.Converters.Add(new DataConverter());
                        opts.Converters.Add(new ErrorConverter());
                    },
                    true
                );
            container.AddHttpRequestFactory(true);
        });
        _httpRequestFactory = Get<IHttpRequestFactory>();
        _serializer = Get<Serializer>();
    }

    /// <summary>
    /// Tests AsAsync with a valid JSON response that can be deserialized to the target type
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task AsAsync_Type_Valid()
    {
        this.Trace("start");

        // arrange
        var data = new Data(5);
        await using var _ = RunServer(
            async (_, response) =>
            {
                await WriteJsonAsync(response, data);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(ServerUri)
            .Get("/")
            .AsAsync<Data>(ct: TestContext.Current.CancellationToken);

        // assert
        response.IsNotDefault();
        response.IsEqual(data);

        this.Trace("done");
    }

    /// <summary>
    /// Tests AsAsync with invalid JSON that cannot be deserialized, expecting default value
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task AsAsync_Type_Invalid()
    {
        this.Trace("start");

        // arrange
        await using var _ = RunServer(
            async (_, response) =>
            {
                await WriteInvalidJsonAsync(response);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(ServerUri)
            .Get("/")
            .AsAsync<Data>(ct: TestContext.Current.CancellationToken);

        // assert
        response.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Tests AsAsync with default value fallback when JSON is valid and deserializable
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task AsAsync_Type_Default_Valid()
    {
        this.Trace("start");

        // arrange
        var data = new Data(5);
        var defaultData = new Data(7);
        await using var _ = RunServer(
            async (_, response) =>
            {
                await WriteJsonAsync(response, data);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(ServerUri)
            .Get("/")
            .AsAsync(defaultData, ct: TestContext.Current.CancellationToken);

        // assert
        response.IsNotDefault();
        response.IsEqual(data);

        this.Trace("done");
    }

    /// <summary>
    /// Tests AsAsync with default value fallback when JSON is invalid, expecting default value to be returned
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task AsAsync_Type_Default_Invalid()
    {
        this.Trace("start");

        // arrange
        var defaultData = new Data(7);
        await using var _ = RunServer(
            async (_, response) =>
            {
                await WriteInvalidJsonAsync(response);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(ServerUri)
            .Get("/")
            .AsAsync(defaultData, ct: TestContext.Current.CancellationToken);

        // assert
        response.IsNotDefault();
        response.IsEqual(defaultData);

        this.Trace("done");
    }

    /// <summary>
    /// Tests AsAsync with success/failure union type when response contains success data
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task AsAsync_SuccessFailure_Success()
    {
        this.Trace("start");

        // arrange
        var data = new Data(5);
        await using var _ = RunServer(
            async (_, response) =>
            {
                await WriteJsonAsync(response, data);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(ServerUri)
            .Get("/")
            .AsAsync<Data, Error>(ct: TestContext.Current.CancellationToken);

        // assert
        response.IsT0.IsTrue();
        response.AsT0.IsNotDefault();
        response.AsT0.IsEqual(data);

        this.Trace("done");
    }

    /// <summary>
    /// Tests AsAsync with success/failure union type when response contains failure data
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task AsAsync_SuccessFailure_Failure()
    {
        this.Trace("start");

        // arrange
        var error = new Error(HttpFailureReason.Undefined, "some failure");
        await using var _ = RunServer(
            async (_, response) =>
            {
                await WriteJsonAsync(response, error);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(ServerUri)
            .Get("/")
            .AsAsync<Data, Error>(ct: TestContext.Current.CancellationToken);

        // assert
        response.IsT1.IsTrue();
        response.AsT1.IsNotDefault();
        response.AsT1.IsEqual(error);

        this.Trace("done");
    }

    /// <summary>
    /// Tests AsAsync with success/failure union type and default error handler when response contains success data
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task AsAsync_SuccessFailure_Default_Success()
    {
        this.Trace("start");

        // arrange
        var data = new Data(5);
        await using var _ = RunServer(
            async (_, response) =>
            {
                await WriteJsonAsync(response, data);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(ServerUri)
            .Get("/")
            .AsAsync<Data, Error>(
                (r, c, e) => Task.FromResult(new Error(r, e?.Message ?? "failure")),
                ct: TestContext.Current.CancellationToken
            );

        // assert
        response.IsT0.IsTrue();
        response.AsT0.IsNotDefault();
        response.AsT0.IsEqual(data);

        this.Trace("done");
    }

    /// <summary>
    /// Tests AsAsync with success/failure union type and default error handler when response contains failure data
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task AsAsync_SuccessFailure_Default_Failure()
    {
        this.Trace("start");

        // arrange
        var error = new Error(HttpFailureReason.Undefined, "some failure");
        await using var _ = RunServer(
            async (_, response) =>
            {
                await WriteJsonAsync(response, error);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(ServerUri)
            .Get("/")
            .AsAsync<Data, Error>(
                (r, c, e) => Task.FromResult(new Error(r, e?.Message ?? "failure")),
                ct: TestContext.Current.CancellationToken
            );

        // assert
        response.IsT1.IsTrue();
        response.AsT1.IsNotDefault();
        response.AsT1.IsEqual(error);

        this.Trace("done");
    }

    /// <summary>
    /// Tests AsAsync with success/failure union type and default error handler when JSON is invalid, expecting default error
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [Fact]
    public async Task AsAsync_SuccessFailure_Default_Default()
    {
        this.Trace("start");

        // arrange
        await using var _ = RunServer(
            async (_, response) =>
            {
                await WriteInvalidJsonAsync(response);
                response.Ok();
            }
        );

        // act
        this.Trace("send");
        var response = await _httpRequestFactory
            .New(ServerUri)
            .Get("/")
            .AsAsync<Data, Error>(
                (r, c, e) => Task.FromResult(new Error(r, e?.Message ?? "failure")),
                ct: TestContext.Current.CancellationToken
            );

        // assert
        response.IsT1.IsTrue();
        response.AsT1.IsNotDefault();
        response.AsT1.IsEqual(new Error(HttpFailureReason.Parse, "failure"));

        this.Trace("done");
    }

    /// <summary>
    /// Writes a value as JSON to the HTTP response stream
    /// </summary>
    /// <typeparam name="T">The type of value to serialize</typeparam>
    /// <param name="response">The HTTP response to write to</param>
    /// <param name="value">The value to serialize and write</param>
    /// <returns>A task representing the asynchronous write operation</returns>
    private async Task WriteJsonAsync<T>(HttpListenerResponse response, T value)
    {
        var contentType = MediaTypeNames.Application.Json;
        response.ContentType = contentType;
        var data = Encoding.UTF8.GetBytes(_serializer.Serialize(contentType, value));
        await response.OutputStream.WriteAsync(data);
    }

    /// <summary>
    /// Writes invalid JSON (object without proper serialization) to the HTTP response stream
    /// </summary>
    /// <param name="response">The HTTP response to write to</param>
    /// <returns>A task representing the asynchronous write operation</returns>
    private async Task WriteInvalidJsonAsync(HttpListenerResponse response)
    {
        await WriteJsonAsync(response, new object());
    }
}
