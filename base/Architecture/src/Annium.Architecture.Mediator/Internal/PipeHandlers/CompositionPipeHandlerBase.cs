using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Extensions.Composition;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers;

/// <summary>
/// Base class for pipe handlers that perform request composition
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
internal abstract class CompositionPipeHandlerBase<TRequest, TResponse> : ILogSubject
    where TRequest : class
{
    /// <summary>
    /// Gets the logger for this pipe handler
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The composer instance used to compose requests.
    /// </summary>
    private readonly IComposer<TRequest> _composer;

    /// <summary>
    /// Initializes a new instance of the CompositionPipeHandlerBase class
    /// </summary>
    /// <param name="composer">The composer for the request type</param>
    /// <param name="logger">The logger instance</param>
    public CompositionPipeHandlerBase(IComposer<TRequest> composer, ILogger logger)
    {
        _composer = composer;
        Logger = logger;
    }

    /// <summary>
    /// Handles the request by composing it and proceeding with the next handler if composition succeeds
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
        this.Trace("Compose {request}", typeof(TRequest));
        var result = await _composer.ComposeAsync(request);
        if (result.HasErrors)
        {
            this.Trace("Composition of {request} failed", typeof(TRequest));

            return GetResponse(result);
        }

        return await next(request, ct);
    }

    /// <summary>
    /// Gets the response when composition fails
    /// </summary>
    /// <param name="compositionResult">The failed composition result</param>
    /// <returns>The failure response</returns>
    protected abstract TResponse GetResponse(IStatusResult<OperationStatus> compositionResult);
}
