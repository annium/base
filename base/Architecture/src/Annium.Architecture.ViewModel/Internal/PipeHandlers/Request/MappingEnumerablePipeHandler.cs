using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Logging;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Request;

internal class MappingEnumerablePipeHandler<TRequestIn, TRequestOut, TResponse> :
    IPipeRequestHandler<
        IEnumerable<TRequestIn>,
        IEnumerable<TRequestOut>,
        TResponse,
        TResponse
    >,
    ILogSubject
    where TRequestIn : IRequest<TRequestOut>
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