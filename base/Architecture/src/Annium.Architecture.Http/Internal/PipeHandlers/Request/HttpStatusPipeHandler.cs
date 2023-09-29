using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Internal.PipeHandlers.Request;

internal class HttpStatusPipeHandler<TRequest> :
    HttpStatusPipeHandlerBase<TRequest, IStatusResult<OperationStatus>, IResult>,
    IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus>, IResult>
{
    protected override IResult GetResponse(IStatusResult<OperationStatus> response)
    {
        HandleStatus(response.Status, response);

        return Result.New().Join(response);
    }
}