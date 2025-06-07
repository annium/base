using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Composition;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers.RequestResponse;

/// <summary>
/// Composition pipe handler for request-response operations
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
internal class CompositionPipeHandler<TRequest, TResponse>
    : CompositionPipeHandlerBase<TRequest, IStatusResult<OperationStatus, TResponse>>,
        IPipeRequestHandler<
            TRequest,
            TRequest,
            IStatusResult<OperationStatus, TResponse>,
            IStatusResult<OperationStatus, TResponse>
        >
    where TRequest : class
{
    /// <summary>
    /// Initializes a new instance of the CompositionPipeHandler class
    /// </summary>
    /// <param name="composer">The composer for the request type</param>
    /// <param name="logger">The logger instance</param>
    public CompositionPipeHandler(IComposer<TRequest> composer, ILogger logger)
        : base(composer, logger) { }

    /// <summary>
    /// Gets the response when composition fails for request-response operations
    /// </summary>
    /// <param name="compositionResult">The failed composition result</param>
    /// <returns>A status result with default response value</returns>
    protected override IStatusResult<OperationStatus, TResponse> GetResponse(
        IStatusResult<OperationStatus> compositionResult
    )
    {
        return Result.Status(compositionResult.Status, default(TResponse)!).Join(compositionResult);
    }
}
