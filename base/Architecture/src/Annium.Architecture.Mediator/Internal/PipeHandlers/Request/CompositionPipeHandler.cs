using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Composition;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers.Request;

/// <summary>
/// Composition pipe handler for request-only operations
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
internal class CompositionPipeHandler<TRequest>
    : CompositionPipeHandlerBase<TRequest, IStatusResult<OperationStatus>>,
        IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus>, IStatusResult<OperationStatus>>
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
    /// Gets the response when composition fails for request-only operations
    /// </summary>
    /// <param name="compositionResult">The failed composition result</param>
    /// <returns>The composition result as the response</returns>
    protected override IStatusResult<OperationStatus> GetResponse(IStatusResult<OperationStatus> compositionResult)
    {
        return compositionResult;
    }
}
