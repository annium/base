using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers;

internal class LoggingPipeHandler<TRequest, TResponse> : IPipeRequestHandler<TRequest, TRequest, TResponse, TResponse>, ILogSubject
{
    public ILogger Logger { get; }

    public LoggingPipeHandler(
        ILogger logger
    )
    {
        Logger = logger;
    }

    public async Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken ct,
        Func<TRequest, CancellationToken, Task<TResponse>> next
    )
    {
        this.Trace("Start {request} -> {response}", typeof(TRequest), typeof(TResponse));

        var result = await next(request, ct);

        this.Trace("Complete {request} -> {response}", typeof(TRequest), typeof(TResponse));

        return result;
    }
}