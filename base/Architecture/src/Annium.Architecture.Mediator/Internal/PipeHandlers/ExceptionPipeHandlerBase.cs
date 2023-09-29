using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Architecture.Mediator.Internal.PipeHandlers;

internal abstract class ExceptionPipeHandlerBase<TRequest, TResponse> : ILogSubject
{
    public ILogger Logger { get; }

    protected ExceptionPipeHandlerBase(
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
        try
        {
            return await next(request, ct);
        }
        catch (TargetInvocationException exception)
        {
            return Failure(exception.InnerException!);
        }
        catch (Exception exception)
        {
            return Failure(exception);
        }
    }

    protected abstract TResponse GetFailure(Exception exception);

    private TResponse Failure(Exception exception)
    {
        this.Trace("Failure of {request}: {exception}", typeof(TRequest), exception);

        return GetFailure(exception);
    }
}