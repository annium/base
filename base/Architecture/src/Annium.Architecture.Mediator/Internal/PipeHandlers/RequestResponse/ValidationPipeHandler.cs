using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers.RequestResponse;

internal class ValidationPipeHandler<TRequest, TResponse> : ValidationPipeHandlerBase<TRequest, IStatusResult<OperationStatus, TResponse>>,
    IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus, TResponse>, IStatusResult<OperationStatus, TResponse>>
{
    public ValidationPipeHandler(
        IValidator<TRequest> validator,
        ILogger logger
    ) : base(validator, logger)
    {
    }

    protected override IStatusResult<OperationStatus, TResponse> GetResponse(IResult validationResult)
    {
        return Result.Status(OperationStatus.BadRequest, default(TResponse)!).Join(validationResult);
    }
}