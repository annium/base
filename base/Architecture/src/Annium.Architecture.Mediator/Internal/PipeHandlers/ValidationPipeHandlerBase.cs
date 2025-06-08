using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Extensions.Validation;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers;

/// <summary>
/// Base class for pipe handlers that perform request validation
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
internal abstract class ValidationPipeHandlerBase<TRequest, TResponse> : ILogSubject
{
    /// <summary>
    /// Gets the logger for this pipe handler
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The validator instance used to validate requests.
    /// </summary>
    private readonly IValidator<TRequest> _validator;

    /// <summary>
    /// Initializes a new instance of the ValidationPipeHandlerBase class
    /// </summary>
    /// <param name="validator">The validator for the request type</param>
    /// <param name="logger">The logger instance</param>
    protected ValidationPipeHandlerBase(IValidator<TRequest> validator, ILogger logger)
    {
        _validator = validator;
        Logger = logger;
    }

    /// <summary>
    /// Handles the request by validating it and proceeding with the next handler if validation succeeds
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="ct">The cancellation token</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <returns>The response from the pipeline</returns>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<TResponse>> next
    )
    {
        this.Trace("Validate {request}", typeof(TRequest));
        if (request is null)
        {
            this.Trace("Validation of {request} failed - request is null", typeof(TRequest));

            return GetResponse(Result.New().Error("Request is empty"));
        }

        var result = await _validator.ValidateAsync(request);
        if (result.HasErrors)
        {
            this.Trace("Validation of {request} failed", typeof(TRequest));

            return GetResponse(result);
        }

        return await next(request, ct);
    }

    /// <summary>
    /// Gets the response when validation fails
    /// </summary>
    /// <param name="validationResult">The failed validation result</param>
    /// <returns>The failure response</returns>
    protected abstract TResponse GetResponse(IResult validationResult);
}
