using System;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers.RequestResponse;

internal class ExceptionPipeHandler<TRequest, TResponse> : ExceptionPipeHandlerBase<TRequest, IStatusResult<OperationStatus, TResponse>>,
    IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus, TResponse>, IStatusResult<OperationStatus, TResponse>>
{
    public ExceptionPipeHandler(
        ILogger logger
    ) : base(logger)
    {
    }

    protected override IStatusResult<OperationStatus, TResponse> GetFailure(Exception exception)
    {
        return Result.Status(OperationStatus.UncaughtError, default(TResponse)!).Error(exception.Message);
    }
}