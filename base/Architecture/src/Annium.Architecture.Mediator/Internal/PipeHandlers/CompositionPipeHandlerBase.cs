using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Extensions.Composition;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers;

internal abstract class CompositionPipeHandlerBase<TRequest, TResponse> : ILogSubject
    where TRequest : class
{
    public ILogger Logger { get; }
    private readonly IComposer<TRequest> _composer;

    public CompositionPipeHandlerBase(
        IComposer<TRequest> composer,
        ILogger logger
    )
    {
        _composer = composer;
        Logger = logger;
    }

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

    protected abstract TResponse GetResponse(IStatusResult<OperationStatus> compositionResult);
}