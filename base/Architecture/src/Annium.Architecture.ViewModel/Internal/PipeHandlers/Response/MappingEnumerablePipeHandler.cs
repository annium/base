using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Response;

internal class MappingEnumerablePipeHandler<TRequest, TResponseIn, TResponseOut> :
    IPipeRequestHandler<
        TRequest,
        TRequest,
        IStatusResult<OperationStatus, IEnumerable<TResponseIn>>,
        IStatusResult<OperationStatus, IEnumerable<TResponseOut>>
    >,
    ILogSubject
    where TResponseOut : IResponse<TResponseIn>
{
    public ILogger Logger { get; }
    private readonly IMapper _mapper;

    public MappingEnumerablePipeHandler(
        IMapper mapper,
        ILogger logger
    )
    {
        _mapper = mapper;
        Logger = logger;
    }

    public async Task<IStatusResult<OperationStatus, IEnumerable<TResponseOut>>> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<IStatusResult<OperationStatus, IEnumerable<TResponseIn>>>> next
    )
    {
        var response = await next(request, ct);

        this.Trace("Map response: {responseIn} -> {responseOut}", typeof(TResponseIn), typeof(TResponseOut));
        var mappedResponse = _mapper.Map<IEnumerable<TResponseOut>>(response.Data);

        return Result.Status(response.Status, mappedResponse).Join(response);
    }
}