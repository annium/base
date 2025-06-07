using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers;

/// <summary>
/// Pipe handler that logs the start and completion of request handling
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
internal class LoggingPipeHandler<TRequest, TResponse>
    : IPipeRequestHandler<TRequest, TRequest, TResponse, TResponse>,
        ILogSubject
{
    /// <summary>
    /// Gets the logger for this pipe handler
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the LoggingPipeHandler class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public LoggingPipeHandler(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Handles the request by logging its start and completion
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="ct">The cancellation token</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <returns>The response from the next handler</returns>
    public async Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<TResponse>> next
    )
    {
        this.Trace("Start {request} -> {response}", typeof(TRequest), typeof(TResponse));

        var result = await next(request, ct);

        this.Trace("Complete {request} -> {response}", typeof(TRequest), typeof(TResponse));

        return result;
    }
}
