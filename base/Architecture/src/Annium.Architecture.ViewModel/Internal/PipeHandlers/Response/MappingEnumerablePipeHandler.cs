using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Response;

/// <summary>
/// Pipe handler that maps enumerable responses from underlying types to view model types
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponseIn">The input underlying response type</typeparam>
/// <typeparam name="TResponseOut">The output view model response type</typeparam>
internal class MappingEnumerablePipeHandler<TRequest, TResponseIn, TResponseOut>
    : MappingPipeHandlerBase,
        IPipeRequestHandler<
            TRequest,
            TRequest,
            IStatusResult<OperationStatus, IEnumerable<TResponseIn>>,
            IStatusResult<OperationStatus, IEnumerable<TResponseOut>>
        >
    where TResponseOut : IResponse<TResponseIn>
{
    /// <summary>
    /// Initializes a new instance of the MappingEnumerablePipeHandler class
    /// </summary>
    /// <param name="mapper">The mapper instance</param>
    /// <param name="logger">The logger instance</param>
    public MappingEnumerablePipeHandler(IMapper mapper, ILogger logger)
        : base(mapper, logger) { }

    /// <summary>
    /// Handles the request by executing the next handler and mapping the enumerable response to view model types
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="ct">The cancellation token</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <returns>The mapped enumerable response</returns>
    public async Task<IStatusResult<OperationStatus, IEnumerable<TResponseOut>>> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<IStatusResult<OperationStatus, IEnumerable<TResponseIn>>>> next
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
            return Result.Status(response.Status, Enumerable.Empty<TResponseOut>()).Join(response);
        }

        if (response.Data is null)
        {
            // Contract violation: Status=Ok implies a non-null Data payload. TResponseIn has no
            // notnull constraint so we can't enforce this at the type level; surface as a
            // programming error so the ExceptionPipeHandler upstream converts it to UncaughtError.
            throw new InvalidOperationException(
                $"Upstream handler returned Status=Ok with null Data for IEnumerable<{typeof(TResponseIn).Name}>"
            );
        }

        this.Trace("Map response: {responseIn} -> {responseOut}", typeof(TResponseIn), typeof(TResponseOut));
        var mappedResponse = Mapper.Map<IEnumerable<TResponseOut>>(response.Data);

        return Result.Status(response.Status, mappedResponse).Join(response);
    }
}
