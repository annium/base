using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Response;

/// <summary>
/// Pipe handler that maps single responses from underlying types to view model types
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponseIn">The input underlying response type</typeparam>
/// <typeparam name="TResponseOut">The output view model response type</typeparam>
internal class MappingSinglePipeHandler<TRequest, TResponseIn, TResponseOut>
    : IPipeRequestHandler<
        TRequest,
        TRequest,
        IStatusResult<OperationStatus, TResponseIn>,
        IStatusResult<OperationStatus, TResponseOut>
    >,
        ILogSubject
    where TResponseOut : IResponse<TResponseIn>
{
    /// <summary>
    /// Gets the logger for this pipe handler
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The mapper instance used to map between types.
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the MappingSinglePipeHandler class
    /// </summary>
    /// <param name="mapper">The mapper instance</param>
    /// <param name="logger">The logger instance</param>
    public MappingSinglePipeHandler(IMapper mapper, ILogger logger)
    {
        _mapper = mapper;
        Logger = logger;
    }

    /// <summary>
    /// Handles the request by executing the next handler and mapping the single response to view model type
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="ct">The cancellation token</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <returns>The mapped single response</returns>
    public async Task<IStatusResult<OperationStatus, TResponseOut>> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<IStatusResult<OperationStatus, TResponseIn>>> next
    )
    {
        var response = await next(request, ct);

        if (response.Status != OperationStatus.Ok)
        {
            this.Trace(
                "Skip mapping on non-Ok status {status}: {responseIn} -> {responseOut}",
                response.Status,
                typeof(TResponseIn),
                typeof(TResponseOut)
            );
            // non-Ok status: default is the explicit "no mapped value" sentinel — sibling
            // MappingEnumerablePipeHandler uses Enumerable.Empty<T>() for the same purpose
            return Result.Status(response.Status, default(TResponseOut)!).Join(response);
        }

        if (response.Data is null)
        {
            // Contract violation: Status=Ok implies a non-null Data payload. TResponseIn has no
            // notnull constraint so we can't enforce this at the type level; surface as a
            // programming error so the ExceptionPipeHandler upstream converts it to UncaughtError.
            throw new InvalidOperationException(
                $"Upstream handler returned Status=Ok with null Data for {typeof(TResponseIn).Name}"
            );
        }

        this.Trace("Map response: {responseIn} -> {responseOut}", typeof(TResponseIn), typeof(TResponseOut));
        var mappedResponse = _mapper.Map<TResponseOut>(response.Data);

        return Result.Status(response.Status, mappedResponse).Join(response);
    }
}
