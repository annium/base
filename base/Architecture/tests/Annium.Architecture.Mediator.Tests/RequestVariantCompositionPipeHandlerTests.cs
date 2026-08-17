using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Composition;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// Tests for the request-only (single-type-parameter) composition pipe handler.
/// The response type is <c>IStatusResult&lt;OperationStatus&gt;</c> with no data type parameter,
/// exercising <c>CompositionPipeHandler&lt;TRequest&gt;.GetResponse</c>.
/// </summary>
public class RequestVariantCompositionPipeHandlerTests : TestBase
{
    /// <summary>
    /// Initializes a new instance and wires the request-only composition pipe handler together with
    /// a final handler that returns <c>IStatusResult&lt;OperationStatus&gt;</c>.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public RequestVariantCompositionPipeHandlerTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        RegisterMediator(cfg => cfg.AddCompositionHandler().AddHandler(typeof(RequestVariantEchoRequestHandler<>)));
    }

    /// <summary>
    /// Verifies that when composition fails, the request-only composition pipe handler returns the
    /// composition result directly (status <c>NotFound</c> with labeled errors).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CompositionFailure_ReturnsCompositionResult()
    {
        // arrange
        var mediator = Get<IMediator>();
        var request = new RequestVariantCompositionRequest { ShouldCompose = false };

        // act
        var result = await mediator.SendAsync<IStatusResult<OperationStatus>>(
            request,
            TestContext.Current.CancellationToken
        );

        // assert
        result.Status.Is(OperationStatus.NotFound);
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At(nameof(RequestVariantCompositionRequest.Tag)).Has(1);
    }

    /// <summary>
    /// Verifies that when composition succeeds, the request-only composition pipe handler passes
    /// the request through to the downstream handler and returns an <c>Ok</c> status.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CompositionSuccess_CallsNext_ReturnsOk()
    {
        // arrange
        var mediator = Get<IMediator>();
        var request = new RequestVariantCompositionRequest { ShouldCompose = true };

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
    /// Request type used by request-only composition pipe handler tests.
    /// </summary>
    private class RequestVariantCompositionRequest : IRequestVariantTagged, IThrowing
    {
        /// <summary>
        /// Gets or sets a value indicating whether composition should succeed.
        /// </summary>
        public bool ShouldCompose { get; set; }

        /// <summary>
        /// Gets or sets the tag value filled by the composer.
        /// </summary>
        public string Tag { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the handler should throw an exception.
        /// </summary>
        public bool Throw => false;
    }

    /// <summary>
    /// Interface for objects that expose a composable <c>Tag</c> field and a composition-control flag.
    /// </summary>
    private interface IRequestVariantTagged
    {
        /// <summary>
        /// Gets or sets a value indicating whether composition should succeed.
        /// </summary>
        bool ShouldCompose { get; set; }

        /// <summary>
        /// Gets or sets the tag value.
        /// </summary>
        string Tag { get; set; }
    }

    /// <summary>
    /// Composer that populates <c>Tag</c> when <c>ShouldCompose</c> is true, or returns null to trigger a
    /// composition failure when it is false.
    /// </summary>
    // ReSharper disable once UnusedType.Local
    private class RequestVariantTagComposer : Composer<IRequestVariantTagged>
    {
        /// <summary>
        /// Initializes a new instance and configures the composition rule for the <c>Tag</c> field.
        /// </summary>
        public RequestVariantTagComposer()
        {
            Field(e => e.Tag).LoadWith(ctx => ctx.Root.ShouldCompose ? ctx.Label.ToLower() : null!);
        }
    }
}
