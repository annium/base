using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Data.Operations.Serialization.Json;
using Annium.Logging;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mediator.Tests;

/// <summary>
/// Tests for mediator functionality. Each test materializes its own <see cref="Fixture"/> so that
/// per-test mediator configurations don't collide with the registration window enforced by
/// <see cref="Annium.Testing.TestBase"/>.
/// </summary>
public class MediatorTest
{
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public MediatorTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    /// <summary>
    /// Tests that a single closed handler works correctly.
    /// </summary>
    [Fact]
    public async Task SingleClosedHandler_Works()
    {
        await using var fx = await BuildAsync(cfg => cfg.AddHandler(typeof(ClosedFinalHandler)));

        var mediator = fx.Get<IMediator>();
        var request = new Base { Value = "base" };

        var response = await mediator.SendAsync<One>(request, TestContext.Current.CancellationToken);

        response.GetHashCode().Is(new One { First = request.Value.Length, Value = request.Value }.GetHashCode());
    }

    /// <summary>
    /// Tests that a single open handler works correctly with expected parameters.
    /// </summary>
    [Fact]
    public async Task SingleOpenHandler_WithExpectedParameters_Works()
    {
        await using var fx = await BuildAsync(cfg => cfg.AddHandler(typeof(OpenFinalHandler<,>)));

        var mediator = fx.Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };

        var response = await mediator.SendAsync<Base>(request, TestContext.Current.CancellationToken);

        response.GetHashCode().Is(new Base { Value = "one_two_three" }.GetHashCode());
    }

    /// <summary>
    /// Tests that a chain of handlers works correctly with expected parameters.
    /// </summary>
    [Fact]
    public async Task ChainOfHandlers_WithExpectedParameters_Works()
    {
        await using var fx = await BuildAsync(cfg =>
            cfg.AddHandler(typeof(ConversionHandler<,>))
                .AddHandler(typeof(ValidationHandler<,>))
                .AddHandler(typeof(OpenFinalHandler<,>))
        );

        var mediator = fx.Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };
        var payload = new Request<Two>(request);

        var response = (
            await mediator.SendAsync<Response<IBooleanResult<Base>>>(payload, TestContext.Current.CancellationToken)
        ).Value;

        response.IsSuccess.IsTrue();
        response.Data.GetHashCode().Is(new Base { Value = "one_two_three" }.GetHashCode());
    }

    /// <summary>
    /// Tests that a chain of handlers works correctly with registered responses.
    /// </summary>
    [Fact]
    public async Task ChainOfHandlers_WithRegisteredResponse_Works()
    {
        await using var fx = await BuildAsync(cfg =>
            cfg.AddHandler(typeof(ConversionHandler<,>))
                .AddHandler(typeof(ValidationHandler<,>))
                .AddHandler(typeof(OpenFinalHandler<,>))
                .AddMatch(typeof(Request<Two>), typeof(IResponse), typeof(Response<IBooleanResult<Base>>))
        );

        var mediator = fx.Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };
        var payload = new Request<Two>(request);

        var response = (await mediator.SendAsync<IResponse>(payload, TestContext.Current.CancellationToken))
            .As<Response<IBooleanResult<Base>>>()
            .Value;

        response.IsSuccess.IsTrue();
        response.Data.GetHashCode().Is(new Base { Value = "one_two_three" }.GetHashCode());
    }

    /// <summary>
    /// Builds an initialized fixture with the supplied mediator configuration.
    /// </summary>
    private async Task<Fixture> BuildAsync(Action<MediatorConfiguration> configure)
    {
        var fx = new Fixture(_outputHelper);
        fx.Configure(configure);
        await fx.InitializeAsync();
        return fx;
    }

    /// <summary>
    /// Per-test fixture inheriting <see cref="Annium.Testing.TestBase"/>; configures the mediator
    /// registrations once via <see cref="Configure"/> before <see cref="Annium.Testing.TestBase.InitializeAsync"/>
    /// is invoked.
    /// </summary>
    private sealed class Fixture(ITestOutputHelper outputHelper) : TestBase(outputHelper), IAsyncDisposable
    {
        /// <summary>
        /// Registers mediator handlers + validators + logging routes. Must be called before
        /// <see cref="Annium.Testing.TestBase.InitializeAsync"/>.
        /// </summary>
        public void Configure(Action<MediatorConfiguration> configure)
        {
            Register(container =>
            {
                container.Add<Func<One, bool>>(value => value.First % 2 == 1).AsSelf().Singleton();
                container.Add<Func<Two, bool>>(value => value.Second % 2 == 0).AsSelf().Singleton();
                container.AddMediatorConfiguration(configure);
                container.AddMediator();
            });
            Setup(sp =>
            {
                sp.UseLogging(route =>
                    route
                        .For(m =>
                            m.SubjectType.StartsWith("ConversionHandler")
                            || m.SubjectType.StartsWith("ValidationHandler")
                            || m.SubjectType.StartsWith("OpenFinalHandler")
                            || m.SubjectType.StartsWith("ClosedFinalHandler")
                        )
                        .UseInMemory<DefaultLogContext>()
                );
            });
        }
    }

    /// <summary>
    /// Handler that converts between request and response types using JSON serialization.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    private class ConversionHandler<TRequest, TResponse>
        : IPipeRequestHandler<Request<TRequest>, TRequest, TResponse, Response<TResponse>>,
            ILogSubject
    {
        /// <summary>JSON serializer options configured for operations.</summary>
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureForOperations();

        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversionHandler{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public ConversionHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Response<TResponse>> HandleAsync(
            Request<TRequest> request,
            CancellationToken ct,
            Func<TRequest, CancellationToken, Task<TResponse>> next
        )
        {
            this.Trace<string>("Deserialize Request to {request}", typeof(TRequest).FriendlyName());
            var payload = JsonSerializer.Deserialize<TRequest>(request.Value, _options)!;

            var result = await next(payload, ct);

            this.Trace<string>("Serialize {response} to Response", typeof(TResponse).FriendlyName());
            return new Response<TResponse>(JsonSerializer.Serialize(result, _options));
        }
    }

    /// <summary>Request wrapper that serializes the value using JSON.</summary>
    /// <typeparam name="T">The type of the request value.</typeparam>
    private class Request<T>
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureForOperations();

        /// <summary>Gets the serialized value.</summary>
        public string Value { get; }

        /// <summary>Initializes a new instance of the <see cref="Request{T}"/> class.</summary>
        public Request(T value)
        {
            Value = JsonSerializer.Serialize(value, _options);
        }
    }

    /// <summary>Response wrapper that deserializes the value using JSON.</summary>
    /// <typeparam name="T">The type of the response value.</typeparam>
    private class Response<T> : IResponse
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureForOperations();

        /// <summary>Gets the deserialized value.</summary>
        public T Value { get; }

        /// <summary>Initializes a new instance of the <see cref="Response{T}"/> class.</summary>
        public Response(string value)
        {
            Value = JsonSerializer.Deserialize<T>(value, _options)!;
        }
    }

    /// <summary>Marker interface for response types.</summary>
    private interface IResponse;

    /// <summary>Handler that validates requests before processing.</summary>
    private class ValidationHandler<TRequest, TResponse>
        : IPipeRequestHandler<TRequest, TRequest, TResponse, IBooleanResult<TResponse>>,
            ILogSubject
    {
        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        private readonly Func<TRequest, bool> _validate;

        /// <summary>Initializes a new instance of the <see cref="ValidationHandler{TRequest, TResponse}"/> class.</summary>
        public ValidationHandler(Func<TRequest, bool> validate, ILogger logger)
        {
            _validate = validate;
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IBooleanResult<TResponse>> HandleAsync(
            TRequest request,
            CancellationToken ct,
            Func<TRequest, CancellationToken, Task<TResponse>> next
        )
        {
            this.Trace<string>("Start {request} validation", typeof(TRequest).FriendlyName());
            var result = _validate(request)
                ? Result.Success(default(TResponse)!)
                : Result.Failure(default(TResponse)!).Error("Validation failed");
            this.Trace(
                "Status of {request} validation: {isSuccess}",
                typeof(TRequest).FriendlyName(),
                result.IsSuccess
            );
            if (result.HasErrors)
                return result;

            var response = await next(request, ct);

            return Result.Success(response);
        }
    }

    /// <summary>Final handler for open generic requests that transforms the request value.</summary>
    private class OpenFinalHandler<TRequest, TResponse> : IFinalRequestHandler<TRequest, TResponse>, ILogSubject
        where TRequest : TResponse
        where TResponse : Base, new()
    {
        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        /// <summary>Initializes a new instance of the <see cref="OpenFinalHandler{TRequest, TResponse}"/> class.</summary>
        public OpenFinalHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <inheritdoc/>
        public Task<TResponse> HandleAsync(TRequest request, CancellationToken ct)
        {
            this.Info<string>("handler: {type}", GetType().FriendlyName());
            this.Trace<int>("request hash: {hash}", request.GetHashCode());

            var response = new TResponse { Value = request.Value!.Replace(' ', '_') };

            return Task.FromResult(response);
        }
    }

    /// <summary>Final handler for closed requests that converts Base to One.</summary>
    private class ClosedFinalHandler : IFinalRequestHandler<Base, One>, ILogSubject
    {
        /// <summary>Gets the logger for this handler.</summary>
        public ILogger Logger { get; }

        /// <summary>Initializes a new instance of the <see cref="ClosedFinalHandler"/> class.</summary>
        public ClosedFinalHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <inheritdoc/>
        public Task<One> HandleAsync(Base request, CancellationToken ct)
        {
            this.Trace<string>("handler: {type}", GetType().FullName!);
            this.Trace<int>("request hash: {hash}", request.GetHashCode());

            return Task.FromResult(new One { First = request.Value!.Length, Value = request.Value });
        }
    }

    /// <summary>Base class for test requests and responses.</summary>
    private class Base
    {
        /// <summary>Gets or sets the value.</summary>
        public string? Value { get; init; }

        /// <inheritdoc/>
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
    }

    /// <summary>Derived class representing a response with a First property.</summary>
    private class One : Base
    {
        /// <summary>Gets or sets the first value.</summary>
        public long First { get; init; }

        /// <inheritdoc/>
        public override int GetHashCode() => 7 * base.GetHashCode() + First.GetHashCode();
    }

    /// <summary>Derived class representing a request with a Second property.</summary>
    private class Two : Base
    {
        /// <summary>Gets or sets the second value.</summary>
        public int Second { get; init; }

        /// <inheritdoc/>
        public override int GetHashCode() => 11 * base.GetHashCode() + Second.GetHashCode();
    }
}
