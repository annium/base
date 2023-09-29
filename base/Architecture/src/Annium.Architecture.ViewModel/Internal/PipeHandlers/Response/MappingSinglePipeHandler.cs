using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Logging;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Response;

internal class MappingSinglePipeHandler<TRequest, TResponseIn, TResponseOut> :
    IPipeRequestHandler<
        TRequest,
        TRequest,
        IStatusResult<OperationStatus, TResponseIn>,
        IStatusResult<OperationStatus, TResponseOut>
    >,
    ILogSubject
    where TResponseOut : IResponse<TResponseIn>
{
    public ILogger Logger { get; }
    private readonly IMapper _mapper;

    public MappingSinglePipeHandler(
        IMapper mapper,
        ILogger logger
    )
    {
        _mapper = mapper;
        Logger = logger;
    }

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