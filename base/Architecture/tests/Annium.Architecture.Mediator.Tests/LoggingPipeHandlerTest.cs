using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// Tests for the logging pipe handler functionality.
/// </summary>
public class LoggingPipeHandlerTest : TestBase
{
    public LoggingPipeHandlerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that the logging handler returns the original result.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ReturnsOriginalResult()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddLoggingHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest();

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.Ok);
        result.IsOk.IsTrue();
    }

    /// <summary>
    /// Test request class for logging testing.
    /// </summary>
    private class LoginRequest : IThrowing
    {
        /// <summary>
        /// Gets or sets a value indicating whether an exception should be thrown.
        /// </summary>
        public bool Throw { get; set; }
    }
}
