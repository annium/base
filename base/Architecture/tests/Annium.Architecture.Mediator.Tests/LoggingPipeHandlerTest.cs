using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Architecture.Mediator.Tests;

public class LoggingPipeHandlerTest : TestBase
{
    public LoggingPipeHandlerTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task ReturnsOriginalResult()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddLoggingHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest();

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