using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Mediator;
using Annium.Logging;

namespace Annium.Architecture.ViewModel.Internal.PipeHandlers.Request;

internal class MappingSinglePipeHandler<TRequestIn, TRequestOut, TResponse> :
    IPipeRequestHandler<
        TRequestIn,
        TRequestOut,
        TResponse,
        TResponse
    >,
    ILogSubject
    where TRequestIn : IRequest<TRequestOut>
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

    public Task<TResponse> HandleAsync(
        TRequestIn request,
        CancellationToken ct,
        Func<TRequestOut, CancellationToken, Task<TResponse>> next
    )
    {
        this.Trace("Map request: {requestIn} -> {requestOut}", typeof(TRequestIn), typeof(TRequestOut));
        var mappedRequest = _mapper.Map<TRequestOut>(request);

        return next(mappedRequest, ct);
    }
}