using System;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers.RequestResponse;

/// <summary>
/// Exception pipe handler for request-response operations
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
internal class ExceptionPipeHandler<TRequest, TResponse>
    : ExceptionPipeHandlerBase<TRequest, IStatusResult<OperationStatus, TResponse>>,
        IPipeRequestHandler<
            TRequest,
            TRequest,
            IStatusResult<OperationStatus, TResponse>,
            IStatusResult<OperationStatus, TResponse>
        >
{
    /// <summary>
    /// Initializes a new instance of the ExceptionPipeHandler class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public ExceptionPipeHandler(ILogger logger)
        : base(logger) { }

    /// <summary>
    /// Gets the failure response when an exception occurs in request-response operations
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>A status result indicating an uncaught error with default response value</returns>
    protected override IStatusResult<OperationStatus, TResponse> GetFailure(Exception exception)
    {
        return Result.Status(OperationStatus.UncaughtError, default(TResponse)!).Error(exception.Message);
    }
}
