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
/// Tests for mediator functionality in the application.
/// </summary>
/// <remarks>
/// Verifies that the mediator can:
/// - Handle single closed handlers
/// - Handle single open handlers
/// - Process chains of handlers
/// - Handle registered responses
/// - Validate request parameters
/// - Convert request types
/// </remarks>
public class MediatorTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    /// <remarks>
    /// Registers the mediator in the dependency injection container.
    /// </remarks>
    public MediatorTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that a single closed handler works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - A single handler can process a request
    /// - The handler returns the expected response
    /// - The mediator correctly routes the request
    /// - The response is properly returned
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SingleClosedHandler_Works()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddHandler(typeof(ClosedFinalHandler)));
        SetupLogging();
        var mediator = Get<IMediator>();
        var request = new Base { Value = "base" };

        // act
        var response = await mediator.SendAsync<One>(request, TestContext.Current.CancellationToken);

        // assert
        response.GetHashCode().Is(new One { First = request.Value.Length, Value = request.Value }.GetHashCode());
    }

    /// <summary>
    /// Tests that a single open handler works correctly with expected parameters.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - An open handler can process a request
    /// - The handler correctly validates parameters
    /// - The mediator routes the request appropriately
    /// - The response matches the expected format
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SingleOpenHandler_WithExpectedParameters_Works()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddHandler(typeof(OpenFinalHandler<,>)));
        SetupLogging();
        var mediator = Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };

        // act
        var response = await mediator.SendAsync<Base>(request, TestContext.Current.CancellationToken);

        // assert
        response.GetHashCode().Is(new Base { Value = "one_two_three" }.GetHashCode());
    }

    /// <summary>
    /// Tests that a chain of handlers works correctly with expected parameters.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Multiple handlers can process a request in sequence
    /// - Each handler correctly processes its part
    /// - The chain maintains data integrity
    /// - The final response is correct
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ChainOfHandlers_WithExpectedParameters_Works()
    {
        // arrange
        RegisterMediator(cfg =>
            cfg.AddHandler(typeof(ConversionHandler<,>))
                .AddHandler(typeof(ValidationHandler<,>))
                .AddHandler(typeof(OpenFinalHandler<,>))
        );
        SetupLogging();
        var mediator = Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };
        var payload = new Request<Two>(request);

        // act
        var response = (
            await mediator.SendAsync<Response<IBooleanResult<Base>>>(payload, TestContext.Current.CancellationToken)
        ).Value;

        // assert
        response.IsSuccess.IsTrue();
        response.Data.GetHashCode().Is(new Base { Value = "one_two_three" }.GetHashCode());
    }

    /// <summary>
    /// Tests that a chain of handlers works correctly with registered responses.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Multiple handlers can process a request with registered responses
    /// - Each handler correctly processes its part
    /// - Registered responses are properly handled
    /// - The final response is correct
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ChainOfHandlers_WithRegisteredResponse_Works()
    {
        // arrange
        RegisterMediator(cfg =>
            cfg.AddHandler(typeof(ConversionHandler<,>))
                .AddHandler(typeof(ValidationHandler<,>))
                .AddHandler(typeof(OpenFinalHandler<,>))
                .AddMatch(typeof(Request<Two>), typeof(IResponse), typeof(Response<IBooleanResult<Base>>))
        );
        SetupLogging();
        var mediator = Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };
        var payload = new Request<Two>(request);

        // act
        var response = (await mediator.SendAsync<IResponse>(payload, TestContext.Current.CancellationToken))
            .As<Response<IBooleanResult<Base>>>()
            .Value;

        // assert
        response.IsSuccess.IsTrue();
        response.Data.GetHashCode().Is(new Base { Value = "one_two_three" }.GetHashCode());
    }

    /// <summary>
    /// Registers the mediator with the specified configuration.
    /// </summary>
    /// <param name="configure">The configuration action to apply.</param>
    private void RegisterMediator(Action<MediatorConfiguration> configure) =>
        Register(container =>
        {
            container.Add<Func<One, bool>>(value => value.First % 2 == 1).AsSelf().Singleton();
            container.Add<Func<Two, bool>>(value => value.Second % 2 == 0).AsSelf().Singleton();

            container.AddMediatorConfiguration(configure);
            container.AddMediator();
        });

    /// <summary>
    /// Sets up logging for the test handlers.
    /// </summary>
    private void SetupLogging() =>
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

    /// <summary>
    /// Handler that converts between request and response types using JSON serialization.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    private class ConversionHandler<TRequest, TResponse>
        : IPipeRequestHandler<Request<TRequest>, TRequest, TResponse, Response<TResponse>>,
            ILogSubject
    {
        /// <summary>
        /// JSON serializer options configured for operations.
        /// </summary>
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureForOperations();

        /// <summary>
        /// Gets the logger for this handler.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversionHandler{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public ConversionHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Handles the request by deserializing it, passing it to the next handler, and serializing the response.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <param name="next">The next handler in the chain.</param>
        /// <returns>The serialized response.</returns>
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

    /// <summary>
    /// Request wrapper that serializes the value using JSON.
    /// </summary>
    /// <typeparam name="T">The type of the request value.</typeparam>
    private class Request<T>
    {
        /// <summary>
        /// JSON serializer options configured for operations.
        /// </summary>
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureForOperations();

        /// <summary>
        /// Gets the serialized value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request{T}"/> class.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        public Request(T value)
        {
            Value = JsonSerializer.Serialize(value, _options);
        }
    }

    /// <summary>
    /// Response wrapper that deserializes the value using JSON.
    /// </summary>
    /// <typeparam name="T">The type of the response value.</typeparam>
    private class Response<T> : IResponse
    {
        /// <summary>
        /// JSON serializer options configured for operations.
        /// </summary>
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureForOperations();

        /// <summary>
        /// Gets the deserialized value.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Response{T}"/> class.
        /// </summary>
        /// <param name="value">The serialized value to deserialize.</param>
        public Response(string value)
        {
            Value = JsonSerializer.Deserialize<T>(value, _options)!;
        }
    }

    /// <summary>
    /// Interface for response types.
    /// </summary>
    private interface IResponse;

    /// <summary>
    /// Handler that validates requests before processing.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    private class ValidationHandler<TRequest, TResponse>
        : IPipeRequestHandler<TRequest, TRequest, TResponse, IBooleanResult<TResponse>>,
            ILogSubject
    {
        /// <summary>
        /// Gets the logger for this handler.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// The validation function to apply to requests.
        /// </summary>
        private readonly Func<TRequest, bool> _validate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationHandler{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="validate">The validation function to use.</param>
        /// <param name="logger">The logger to use.</param>
        public ValidationHandler(Func<TRequest, bool> validate, ILogger logger)
        {
            _validate = validate;
            Logger = logger;
        }

        /// <summary>
        /// Handles the request by validating it and then passing it to the next handler if valid.
        /// </summary>
        /// <param name="request">The request to validate and handle.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <param name="next">The next handler in the chain.</param>
        /// <returns>A result indicating validation success and containing the response if successful.</returns>
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

    /// <summary>
    /// Final handler for open generic requests that transforms the request value.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    private class OpenFinalHandler<TRequest, TResponse> : IFinalRequestHandler<TRequest, TResponse>, ILogSubject
        where TRequest : TResponse
        where TResponse : Base, new()
    {
        /// <summary>
        /// Gets the logger for this handler.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFinalHandler{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public OpenFinalHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Handles the request by transforming the value (replacing spaces with underscores).
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The transformed response.</returns>
        public Task<TResponse> HandleAsync(TRequest request, CancellationToken ct)
        {
            this.Info(GetType().FriendlyName());
            this.Trace(request.GetHashCode().ToString());

            var response = new TResponse { Value = request.Value!.Replace(' ', '_') };

            return Task.FromResult(response);
        }
    }

    /// <summary>
    /// Final handler for closed requests that converts Base to One.
    /// </summary>
    private class ClosedFinalHandler : IFinalRequestHandler<Base, One>, ILogSubject
    {
        /// <summary>
        /// Gets the logger for this handler.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClosedFinalHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        public ClosedFinalHandler(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Handles the request by converting a Base to One with the value length as First.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The converted One response.</returns>
        public Task<One> HandleAsync(Base request, CancellationToken ct)
        {
            this.Trace(GetType().FullName!);
            this.Trace(request.GetHashCode().ToString());

            return Task.FromResult(new One { First = request.Value!.Length, Value = request.Value });
        }
    }

    /// <summary>
    /// Base class for test requests and responses.
    /// </summary>
    private class Base
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string? Value { get; init; }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code based on the Value property.</returns>
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Derived class representing a response with a First property.
    /// </summary>
    private class One : Base
    {
        /// <summary>
        /// Gets or sets the first value.
        /// </summary>
        public long First { get; init; }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code based on the base hash code and First property.</returns>
        public override int GetHashCode() => 7 * base.GetHashCode() + First.GetHashCode();
    }

    /// <summary>
    /// Derived class representing a request with a Second property.
    /// </summary>
    private class Two : Base
    {
        /// <summary>
        /// Gets or sets the second value.
        /// </summary>
        public int Second { get; init; }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code based on the base hash code and Second property.</returns>
        public override int GetHashCode() => 11 * base.GetHashCode() + Second.GetHashCode();
    }
}
