using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers;

/// <summary>
/// Base class for pipe handlers that handle exceptions in the pipeline
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
internal abstract class ExceptionPipeHandlerBase<TRequest, TResponse> : ILogSubject
{
    /// <summary>
    /// Gets the logger for this pipe handler
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the ExceptionPipeHandlerBase class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    protected ExceptionPipeHandlerBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Handles the request by executing the next handler and catching any exceptions
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="ct">The cancellation token</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <returns>The response from the pipeline or a failure response if an exception occurs</returns>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<TResponse>> next
    )
    {
        try
        {
            return await next(request, ct);
        }
        catch (TargetInvocationException exception)
        {
            return Failure(exception.InnerException!);
        }
        catch (Exception exception)
        {
            return Failure(exception);
        }
    }

    /// <summary>
    /// Gets the failure response when an exception occurs
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>The failure response</returns>
    protected abstract TResponse GetFailure(Exception exception);

    /// <summary>
    /// Handles the failure by logging and creating a failure response
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>The failure response</returns>
    private TResponse Failure(Exception exception)
    {
        this.Trace("Failure of {request}: {exception}", typeof(TRequest), exception);

        return GetFailure(exception);
    }
}
