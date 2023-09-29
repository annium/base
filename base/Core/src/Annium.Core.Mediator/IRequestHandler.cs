using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Core.Mediator;

public interface IPipeRequestHandler<TRequestIn, TRequestOut, TResponseIn, TResponseOut> :
    IRequestHandlerInput<TRequestIn, TResponseOut>,
    IRequestHandlerOutput<TRequestOut, TResponseIn>
{
    Task<TResponseOut> HandleAsync(
        TRequestIn request,
        CancellationToken ct,
        Func<TRequestOut, CancellationToken, Task<TResponseIn>> next
    );
}

public interface IFinalRequestHandler<TRequest, TResponse> :
    IRequestHandlerInput<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken ct
    );
}

public interface IRequestHandlerInput<TRequestIn, TResponseOut>
{
}

public interface IRequestHandlerOutput<TRequestOut, TResponseIn>
{
}