using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Composition;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers.RequestResponse;

internal class CompositionPipeHandler<TRequest, TResponse> :
    CompositionPipeHandlerBase<TRequest, IStatusResult<OperationStatus, TResponse>>,
    IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus, TResponse>, IStatusResult<OperationStatus, TResponse>> where TRequest : class
{
    public CompositionPipeHandler(
        IComposer<TRequest> composer,
        ILogger logger
    ) : base(composer, logger)
    {
    }

    protected override IStatusResult<OperationStatus, TResponse> GetResponse(IStatusResult<OperationStatus> compositionResult)
    {
        return Result.Status(compositionResult.Status, default(TResponse)!).Join(compositionResult);
    }
}