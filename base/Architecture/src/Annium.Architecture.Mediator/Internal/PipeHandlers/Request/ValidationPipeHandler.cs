using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers.Request;

internal class ValidationPipeHandler<TRequest> : ValidationPipeHandlerBase<TRequest, IStatusResult<OperationStatus>>,
    IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus>, IStatusResult<OperationStatus>>
{
    public ValidationPipeHandler(
        IValidator<TRequest> validator,
        ILogger logger
    ) : base(validator, logger)
    {
    }

    protected override IStatusResult<OperationStatus> GetResponse(IResult validationResult)
    {
        return Result.Status(OperationStatus.BadRequest).Join(validationResult);
    }
}