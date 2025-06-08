using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Core.Mediator;

/// <summary>
/// Represents a pipeline request handler that transforms requests and responses
/// </summary>
/// <typeparam name="TRequestIn">The input request type</typeparam>
/// <typeparam name="TRequestOut">The output request type</typeparam>
/// <typeparam name="TResponseIn">The input response type</typeparam>
/// <typeparam name="TResponseOut">The output response type</typeparam>
public interface IPipeRequestHandler<TRequestIn, TRequestOut, TResponseIn, TResponseOut>
    : IRequestHandlerInput<TRequestIn, TResponseOut>,
        IRequestHandlerOutput<TRequestOut, TResponseIn>
{
    /// <summary>
    /// Handles the request and delegates to the next handler in the pipeline
    /// </summary>
    /// <param name="request">The input request</param>
    /// <param name="ct">The cancellation token</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <returns>The transformed response</returns>
    Task<TResponseOut> HandleAsync(
        TRequestIn request,
        CancellationToken ct,
        Func<TRequestOut, CancellationToken, Task<TResponseIn>> next
    );
}

/// <summary>
/// Represents a final request handler that terminates the pipeline
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IFinalRequestHandler<TRequest, TResponse> : IRequestHandlerInput<TRequest, TResponse>
{
    /// <summary>
    /// Handles the final request and produces a response
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The response</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct);
}

/// <summary>
/// Marker interface for request handlers that accept a specific input and produce a specific output
/// </summary>
/// <typeparam name="TRequestIn">The input request type</typeparam>
/// <typeparam name="TResponseOut">The output response type</typeparam>
public interface IRequestHandlerInput<TRequestIn, TResponseOut>;

/// <summary>
/// Marker interface for request handlers that output a specific request type and accept a specific response type
/// </summary>
/// <typeparam name="TRequestOut">The output request type</typeparam>
/// <typeparam name="TResponseIn">The input response type</typeparam>
public interface IRequestHandlerOutput<TRequestOut, TResponseIn>;
