using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Logging;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Request;

/// <summary>
/// Pipe handler that maps enumerable view model requests to their underlying types
/// </summary>
/// <typeparam name="TRequestIn">The input view model request type</typeparam>
/// <typeparam name="TRequestOut">The output underlying request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
internal class MappingEnumerablePipeHandler<TRequestIn, TRequestOut, TResponse>
    : IPipeRequestHandler<IEnumerable<TRequestIn>, IEnumerable<TRequestOut>, TResponse, TResponse>,
        ILogSubject
    where TRequestIn : IRequest<TRequestOut>
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
    /// Initializes a new instance of the MappingEnumerablePipeHandler class
    /// </summary>
    /// <param name="mapper">The mapper instance</param>
    /// <param name="logger">The logger instance</param>
    public MappingEnumerablePipeHandler(IMapper mapper, ILogger logger)
    {
        _mapper = mapper;
        Logger = logger;
    }

    /// <summary>
    /// Handles the enumerable request by mapping it from view model type to underlying type
    /// </summary>
    /// <param name="request">The enumerable view model request to map</param>
    /// <param name="ct">The cancellation token</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <returns>The response from the next handler</returns>
    public Task<TResponse> HandleAsync(
        IEnumerable<TRequestIn> request,
        CancellationToken ct,
        Func<IEnumerable<TRequestOut>, CancellationToken, Task<TResponse>> next
    )
    {
        this.Trace("Map request: {requestIn} -> {requestOut}", typeof(TRequestIn), typeof(TRequestOut));
        var mappedRequest = _mapper.Map<IEnumerable<TRequestOut>>(request);

        return next(mappedRequest, ct);
    }
}
