using System;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers.Request;

/// <summary>
/// Exception pipe handler for request-only operations
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
internal class ExceptionPipeHandler<TRequest>
    : ExceptionPipeHandlerBase<TRequest, IStatusResult<OperationStatus>>,
        IPipeRequestHandler<TRequest, TRequest, IStatusResult<OperationStatus>, IStatusResult<OperationStatus>>
{
    /// <summary>
    /// Initializes a new instance of the ExceptionPipeHandler class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public ExceptionPipeHandler(ILogger logger)
        : base(logger) { }

    /// <summary>
    /// Gets the failure response when an exception occurs in request-only operations
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>A status result indicating an uncaught error</returns>
    protected override IStatusResult<OperationStatus> GetFailure(Exception exception)
    {
        return Result.Status(OperationStatus.UncaughtError).Error(exception.Message);
    }
}
