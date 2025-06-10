using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Core.Mediator;

/// <summary>
/// Mediator for handling requests through a pipeline of handlers
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request through the mediator pipeline
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="request">The request to send</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The response from the pipeline</returns>
    Task<TResponse> SendAsync<TResponse>(object request, CancellationToken ct = default);

    /// <summary>
    /// Sends a request through the mediator pipeline with a specific service provider
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="serviceProvider">The service provider to use</param>
    /// <param name="request">The request to send</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The response from the pipeline</returns>
    Task<TResponse> SendAsync<TResponse>(
        IServiceProvider serviceProvider,
        object request,
        CancellationToken ct = default
    );
}
