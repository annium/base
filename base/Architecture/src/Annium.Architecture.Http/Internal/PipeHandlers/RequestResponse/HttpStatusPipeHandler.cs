using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Internal.PipeHandlers.RequestResponse;

internal class HttpStatusPipeHandler<TRequest, TResponse> :
    HttpStatusPipeHandlerBase<TRequest, IStatusResult<OperationStatus, TResponse>, IResult<TResponse>>,
    IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus, TResponse>, IResult<TResponse>>
{
    protected override IResult<TResponse> GetResponse(IStatusResult<OperationStatus, TResponse> response)
    {
        HandleStatus(response.Status, response);

        return Result.New(response.Data).Join(response);
    }
}