using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Testing;
using Annium.Testing.Collection;
using Xunit;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// Tests for the exception pipe handler functionality.
/// </summary>
public class ExceptionPipeHandlerTest : TestBase
{
    public ExceptionPipeHandlerTest(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests that exceptions are caught and returned as uncaught exception results.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Exception_ReturnsUncaughtExceptionResult()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddExceptionHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest { Throw = true };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus, LoginRequest>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.UncaughtError);
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is("TEST EXCEPTION");
    }

    /// <summary>
    /// Tests that successful operations return the original result.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Success_ReturnsOriginalResult()
    {
        // arrange
        RegisterMediator(cfg => cfg.AddCompositionHandler().AddHandler(typeof(EchoRequestHandler<>)));
        var mediator = Get<IMediator>();
        var request = new LoginRequest { Throw = false };

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
    /// Test request class for exception handling testing.
    /// </summary>
    private class LoginRequest : IThrowing
    {
        /// <summary>
        /// Gets or sets a value indicating whether an exception should be thrown.
        /// </summary>
        public bool Throw { get; set; }
    }
}
