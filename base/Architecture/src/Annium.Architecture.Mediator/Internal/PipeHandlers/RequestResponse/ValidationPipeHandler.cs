using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers.RequestResponse;

/// <summary>
/// Validation pipe handler for request-response operations
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
internal class ValidationPipeHandler<TRequest, TResponse>
    : ValidationPipeHandlerBase<TRequest, IStatusResult<OperationStatus, TResponse>>,
        IPipeRequestHandler<
            TRequest,
            TRequest,
            IStatusResult<OperationStatus, TResponse>,
            IStatusResult<OperationStatus, TResponse>
        >
{
    /// <summary>
    /// Initializes a new instance of the ValidationPipeHandler class
    /// </summary>
    /// <param name="validator">The validator for the request type</param>
    /// <param name="logger">The logger instance</param>
    public ValidationPipeHandler(IValidator<TRequest> validator, ILogger logger)
        : base(validator, logger) { }

    /// <summary>
    /// Gets the response when validation fails for request-response operations
    /// </summary>
    /// <param name="status">The operation status to report</param>
    /// <param name="validationResult">The failed validation result</param>
    /// <returns>A status result reporting the given status with default response value and joined errors</returns>
    protected override IStatusResult<OperationStatus, TResponse> GetResponse(
        OperationStatus status,
        IResult validationResult
    )
    {
        return Result.Status(status, default(TResponse)!).Join(validationResult);
    }
}
