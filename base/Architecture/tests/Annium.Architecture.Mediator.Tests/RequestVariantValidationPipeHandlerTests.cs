using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// Tests for the request-only (single-type-parameter) validation pipe handler.
/// The response type is <c>IStatusResult&lt;OperationStatus&gt;</c> with no data type parameter,
/// exercising <c>ValidationPipeHandler&lt;TRequest&gt;.GetResponse</c>.
/// </summary>
public class RequestVariantValidationPipeHandlerTests : TestBase
{
    /// <summary>
    /// Initializes a new instance and wires the request-only validation pipe handler together with
    /// a final handler that returns <c>IStatusResult&lt;OperationStatus&gt;</c>.
    /// </summary>
    public RequestVariantValidationPipeHandlerTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterMediator(cfg => cfg.AddValidationHandler().AddHandler(typeof(RequestVariantEchoRequestHandler<>)));
    }

    /// <summary>
    /// Verifies that when validation fails, the request-only validation pipe handler returns a
    /// <c>BadRequest</c> status with labeled errors and no data payload.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ValidationFailure_ReturnsBadRequest()
    {
        // arrange
        var mediator = Get<IMediator>();
        var request = new RequestVariantValidationRequest();

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.BadRequest);
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At(nameof(RequestVariantValidationRequest.Token)).Has(1);
    }

    /// <summary>
    /// Verifies that when validation succeeds, the request-only validation pipe handler passes the
    /// result through to the downstream handler and returns an <c>Ok</c> status.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ValidationSuccess_CallsNext_ReturnsOk()
    {
        // arrange
        var mediator = Get<IMediator>();
        var request = new RequestVariantValidationRequest { Token = "abc" };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.Ok);
        result.IsOk.IsTrue();
    }

    /// <summary>
    /// Request type used by request-only validation pipe handler tests.
    /// </summary>
    private class RequestVariantValidationRequest : IRequestVariantToken, IThrowing
    {
        /// <summary>
        /// Gets or sets the token value subject to validation.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the handler should throw an exception.
        /// </summary>
        public bool Throw => false;
    }

    /// <summary>
    /// Interface for objects that carry a token field.
    /// </summary>
    private interface IRequestVariantToken
    {
        /// <summary>
        /// Gets the token value.
        /// </summary>
        string Token { get; }
    }

    /// <summary>
    /// Validator that requires the <c>Token</c> field to be non-empty.
    /// </summary>
    // ReSharper disable once UnusedType.Local
    private class RequestVariantTokenValidator : Validator<IRequestVariantToken>
    {
        /// <summary>
        /// Initializes a new instance and configures the validation rule.
        /// </summary>
        public RequestVariantTokenValidator()
        {
            Field(e => e.Token).Required();
        }
    }
}
