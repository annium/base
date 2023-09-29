using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Mediator.Tests;

public class MediatorTest : TestBase
{
    public MediatorTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task SingleClosedHandler_Works()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddHandler(typeof(ClosedFinalHandler)));
        SetupLogging();
        var mediator = Get<IMediator>();
        var request = new Base { Value = "base" };

        // act
        var response = await mediator.SendAsync<One>(request);

        // assert
        response.GetHashCode().Is(new One { First = request.Value.Length, Value = request.Value }.GetHashCode());
    }

    [Fact]
    public async Task SingleOpenHandler_WithExpectedParameters_Works()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddHandler(typeof(OpenFinalHandler<,>)));
        SetupLogging();
        var mediator = Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };

        // act
        var response = await mediator.SendAsync<Base>(request);

        // assert
        response.GetHashCode().Is(new Base { Value = "one_two_three" }.GetHashCode());
    }

    [Fact]
    public async Task ChainOfHandlers_WithExpectedParameters_Works()
    {
        // arrange
        RegisterMediator(cfg => cfg
            .AddHandler(typeof(ConversionHandler<,>))
            .AddHandler(typeof(ValidationHandler<,>))
            .AddHandler(typeof(OpenFinalHandler<,>))
        );
        SetupLogging();
        var mediator = Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };
        var payload = new Request<Two>(request);

        // act
        var response = (await mediator.SendAsync<Response<IBooleanResult<Base>>>(payload)).Value;

        // assert
        response.IsSuccess.IsTrue();
        response.Data.GetHashCode().Is(new Base { Value = "one_two_three" }.GetHashCode());
    }

    [Fact]
    public async Task ChainOfHandlers_WithRegisteredResponse_Works()
    {
        // arrange
        RegisterMediator(cfg => cfg
            .AddHandler(typeof(ConversionHandler<,>))
            .AddHandler(typeof(ValidationHandler<,>))
            .AddHandler(typeof(OpenFinalHandler<,>))
            .AddMatch(typeof(Request<Two>), typeof(IResponse), typeof(Response<IBooleanResult<Base>>))
        );
        SetupLogging();
        var mediator = Get<IMediator>();
        var request = new Two { Second = 2, Value = "one two three" };
        var payload = new Request<Two>(request);

        // act
        var response = (await mediator.SendAsync<IResponse>(payload)).As<Response<IBooleanResult<Base>>>().Value;

        // assert
        response.IsSuccess.IsTrue();
        response.Data.GetHashCode().Is(new Base { Value = "one_two_three" }.GetHashCode());
    }

    private void RegisterMediator(Action<MediatorConfiguration> configure) => Register(container =>
    {
        container.Add<Func<One, bool>>(value => value.First % 2 == 1).AsSelf().Singleton();
        container.Add<Func<Two, bool>>(value => value.Second % 2 == 0).AsSelf().Singleton();

        container.AddMediatorConfiguration(configure);
        container.AddMediator();
    });

    private void SetupLogging() => Setup(sp =>
    {
        sp.UseLogging(route => route
            .For(m =>
                m.SubjectType.StartsWith("ConversionHandler") ||
                m.SubjectType.StartsWith("ValidationHandler") ||
                m.SubjectType.StartsWith("OpenFinalHandler") ||
                m.SubjectType.StartsWith("ClosedFinalHandler")
            )
            .UseInMemory()
        );
    });

    private class ConversionHandler<TRequest, TResponse> :
        IPipeRequestHandler<Request<TRequest>, TRequest, TResponse, Response<TResponse>>,
        ILogSubject
    {
        private readonly JsonSerializerOptions
            _options = new JsonSerializerOptions().ConfigureForOperations();

        public ILogger Logger { get; }

        public ConversionHandler(
            ILogger logger
        )
        {
            Logger = logger;
        }

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

    private class Request<T>
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureForOperations();

        public string Value { get; }

        public Request(T value)
        {
            Value = JsonSerializer.Serialize(value, _options);
        }
    }

    private class Response<T> : IResponse
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureForOperations();

        public T Value { get; }

        public Response(string value)
        {
            Value = JsonSerializer.Deserialize<T>(value, _options)!;
        }
    }

    private interface IResponse
    {
    }

    private class ValidationHandler<TRequest, TResponse> :
        IPipeRequestHandler<TRequest, TRequest, TResponse, IBooleanResult<TResponse>>,
        ILogSubject
    {
        public ILogger Logger { get; }
        private readonly Func<TRequest, bool> _validate;

        public ValidationHandler(
            Func<TRequest, bool> validate,
            ILogger logger
        )
        {
            _validate = validate;
            Logger = logger;
        }

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
            this.Trace("Status of {request} validation: {isSuccess}", typeof(TRequest).FriendlyName(), result.IsSuccess);
            if (result.HasErrors)
                return result;

            var response = await next(request, ct);

            return Result.Success(response);
        }
    }

    private class OpenFinalHandler<TRequest, TResponse> : IFinalRequestHandler<TRequest, TResponse>, ILogSubject
        where TRequest : TResponse
        where TResponse : Base, new()
    {
        public ILogger Logger { get; }

        public OpenFinalHandler(
            ILogger logger
        )
        {
            Logger = logger;
        }

        public Task<TResponse> HandleAsync(
            TRequest request,
            CancellationToken ct
        )
        {
            this.Info(GetType().FriendlyName());
            this.Trace(request.GetHashCode().ToString());

            var response = new TResponse { Value = request.Value!.Replace(' ', '_') };

            return Task.FromResult(response);
        }
    }

    private class ClosedFinalHandler : IFinalRequestHandler<Base, One>, ILogSubject
    {
        public ILogger Logger { get; }

        public ClosedFinalHandler(
            ILogger logger
        )
        {
            Logger = logger;
        }

        public Task<One> HandleAsync(
            Base request,
            CancellationToken ct
        )
        {
            this.Trace(GetType().FullName!);
            this.Trace(request.GetHashCode().ToString());

            return Task.FromResult(new One { First = request.Value!.Length, Value = request.Value });
        }
    }

    private class Base
    {
        public string? Value { get; init; }

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
    }

    private class One : Base
    {
        public long First { get; init; }

        public override int GetHashCode() => 7 * base.GetHashCode() + First.GetHashCode();
    }

    private class Two : Base
    {
        public int Second { get; init; }

        public override int GetHashCode() => 11 * base.GetHashCode() + Second.GetHashCode();
    }
}