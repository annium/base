using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.Mediator.Internal.PipeHandlers;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// Tests for the request-only (single-type-parameter) exception pipe handler.
/// The response type is <c>IStatusResult&lt;OperationStatus&gt;</c> with no data type parameter,
/// exercising <c>ExceptionPipeHandler&lt;TRequest&gt;.GetFailure</c>.
/// </summary>
public class RequestVariantExceptionPipeHandlerTests : TestBase
{
    /// <summary>
    /// Initializes a new instance and wires the request-only exception pipe handler together with
    /// a final handler that returns <c>IStatusResult&lt;OperationStatus&gt;</c>.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public RequestVariantExceptionPipeHandlerTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterMediator(cfg => cfg.AddExceptionHandler().AddHandler(typeof(RequestVariantEchoRequestHandler<>)));
    }

    /// <summary>
    /// Verifies that when the downstream handler throws, the request-only exception pipe handler
    /// returns a result with <c>UncaughtError</c> status and the generic sentinel error message.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Exception_ReturnsUncaughtError()
    {
        // arrange
        var mediator = Get<IMediator>();
        var request = new ThrowingRequest { Throw = true };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.UncaughtError);
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is(PipeHandlerMessages.InternalError);
    }

    /// <summary>
    /// Verifies that when the downstream handler succeeds, the request-only exception pipe handler
    /// passes the result through unchanged with an <c>Ok</c> status.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Success_ReturnsOkResult()
    {
        // arrange
        var mediator = Get<IMediator>();
        var request = new ThrowingRequest { Throw = false };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.Ok);
        result.IsOk.IsTrue();
    }
}
