using System;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers.Request;

internal class ExceptionPipeHandler<TRequest> : ExceptionPipeHandlerBase<TRequest, IStatusResult<OperationStatus>>,
    IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus>, IStatusResult<OperationStatus>>
{
    public ExceptionPipeHandler(
        ILogger logger
    ) : base(logger)
    {
    }

    protected override IStatusResult<OperationStatus> GetFailure(Exception exception)
    {
        return Result.Status(OperationStatus.UncaughtError).Error(exception.Message);
    }
}