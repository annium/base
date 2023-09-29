using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Architecture.Mediator.Tests;

public class ExceptionPipeHandlerTest : TestBase
{
    public ExceptionPipeHandlerTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Exception_ReturnsUncaughtExceptionResult()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddExceptionHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest { Throw = true };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(request);

        // assert
        result.Status.Is(OperationStatus.UncaughtError);
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is("TEST EXCEPTION");
    }

    [Fact]
    public async Task Success_ReturnsOriginalResult()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddCompositionHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest { Throw = false };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(request);

        // assert
        result.Status.Is(OperationStatus.Ok);
        result.IsOk.IsTrue();
    }

    private class LoginRequest : IThrowing
    {
        public bool Throw { get; set; }
    }
}