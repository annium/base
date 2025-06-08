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

        this.Trace("Map response: {responseIn} -> {responseOut}", typeof(TResponseIn), typeof(TResponseOut));
        var mappedResponse = _mapper.Map<TResponseOut>(response.Data!);

        return Result.Status(response.Status, mappedResponse).Join(response);
    }
}
