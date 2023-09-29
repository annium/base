using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Http.Internal;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Http.Tests.Extensions;

public class AsResponseExtensionsTests : TestBase
{
    private readonly IHttpRequestFactory _httpRequestFactory;
    private readonly Serializer _serializer;

    public AsResponseExtensionsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(container =>
        {
            container.AddSerializers().WithJson(opts =>
            {
                opts.Converters.Clear();
                opts.Converters.Add(new DataConverter());
                opts.Converters.Add(new ErrorConverter());
            }, true);
            container.AddHttpRequestFactory(true);
        });
        _httpRequestFactory = Get<IHttpRequestFactory>();
        _serializer = Get<Serializer>();
    }

    [Fact]
    public async Task AsResponseAsync_Type_Valid()
    {
        this.Trace("start");

        // arrange
        var data = new Data(5);
        await using var _ = RunServer(async (_, response) =>
        {
            await WriteJsonAsync(response, data);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri).Get("/").AsResponseAsync<Data>();

        // assert
        response.Data.IsNotDefault();
        response.Data.IsEqual(data);

        this.Trace("done");
    }

    [Fact]
    public async Task AsResponseAsync_Type_Invalid()
    {
        this.Trace("start");

        // arrange
        await using var _ = RunServer(async (_, response) =>
        {
            await WriteInvalidJsonAsync(response);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri).Get("/").AsResponseAsync<Data>();

        // assert
        response.Data.IsDefault();

        this.Trace("done");
    }

    [Fact]
    public async Task AsResponseAsync_Type_Default_Valid()
    {
        this.Trace("start");

        // arrange
        var data = new Data(5);
        var defaultData = new Data(7);
        await using var _ = RunServer(async (_, response) =>
        {
            await WriteJsonAsync(response, data);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri).Get("/").AsResponseAsync(defaultData);

        // assert
        response.Data.IsNotDefault();
        response.Data.IsEqual(data);

        this.Trace("done");
    }

    [Fact]
    public async Task AsResponseAsync_Type_Default_Invalid()
    {
        this.Trace("start");

        // arrange
        var defaultData = new Data(7);
        await using var _ = RunServer(async (_, response) =>
        {
            await WriteInvalidJsonAsync(response);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri).Get("/").AsResponseAsync(defaultData);

        // assert
        response.Data.IsNotDefault();
        response.Data.IsEqual(defaultData);

        this.Trace("done");
    }

    [Fact]
    public async Task AsResponseAsync_SuccessFailure_Success()
    {
        this.Trace("start");

        // arrange
        var data = new Data(5);
        await using var _ = RunServer(async (_, response) =>
        {
            await WriteJsonAsync(response, data);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri).Get("/").AsResponseAsync<Data, Error>();

        // assert
        response.Data.IsT0.IsTrue();
        response.Data.AsT0.IsNotDefault();
        response.Data.AsT0.IsEqual(data);

        this.Trace("done");
    }

    [Fact]
    public async Task AsResponseAsync_SuccessFailure_Failure()
    {
        this.Trace("start");

        // arrange
        var error = new Error("failure");
        await using var _ = RunServer(async (_, response) =>
        {
            await WriteJsonAsync(response, error);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri).Get("/").AsResponseAsync<Data, Error>();

        // assert
        response.Data.IsT1.IsTrue();
        response.Data.AsT1.IsNotDefault();
        response.Data.AsT1.IsEqual(error);

        this.Trace("done");
    }

    [Fact]
    public async Task AsResponseAsync_SuccessFailure_Default_Success()
    {
        this.Trace("start");

        // arrange
        var data = new Data(5);
        var defaultData = new Data(7);
        await using var _ = RunServer(async (_, response) =>
        {
            await WriteJsonAsync(response, data);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri).Get("/").AsResponseAsync<Data, Error>(defaultData);

        // assert
        response.Data.IsT0.IsTrue();
        response.Data.AsT0.IsNotDefault();
        response.Data.AsT0.IsEqual(data);

        this.Trace("done");
    }

    [Fact]
    public async Task AsResponseAsync_SuccessFailure_Default_Failure()
    {
        this.Trace("start");

        // arrange
        var error = new Error("failure");
        var defaultData = new Data(7);
        await using var _ = RunServer(async (_, response) =>
        {
            await WriteJsonAsync(response, error);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri).Get("/").AsResponseAsync<Data, Error>(defaultData);

        // assert
        response.Data.IsT1.IsTrue();
        response.Data.AsT1.IsNotDefault();
        response.Data.AsT1.IsEqual(error);

        this.Trace("done");
    }

    [Fact]
    public async Task AsResponseAsync_SuccessFailure_Default_Default()
    {
        this.Trace("start");

        // arrange
        var defaultData = new Data(7);
        await using var _ = RunServer(async (_, response) =>
        {
            await WriteInvalidJsonAsync(response);
            response.Ok();
        });

        // act
        this.Trace("send");
        var response = await _httpRequestFactory.New(ServerUri).Get("/").AsResponseAsync<Data, Error>(defaultData);

        // assert
        response.Data.IsT0.IsTrue();
        response.Data.AsT0.IsNotDefault();
        response.Data.AsT0.IsEqual(defaultData);

        this.Trace("done");
    }

    private async Task WriteJsonAsync<T>(HttpListenerResponse response, T value)
    {
        var contentType = MediaTypeNames.Application.Json;
        response.ContentType = contentType;
        var data = Encoding.UTF8.GetBytes(_serializer.Serialize(contentType, value));
        await response.OutputStream.WriteAsync(data);
    }

    private async Task WriteInvalidJsonAsync(HttpListenerResponse response)
    {
        await WriteJsonAsync(response, new object());
    }
}