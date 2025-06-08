using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Mediator.Internal;

/// <summary>
/// Internal implementation of the mediator pattern for request handling
/// </summary>
internal class Mediator : IMediator
{
    /// <summary>
    /// Builder for creating execution chains
    /// </summary>
    private readonly ChainBuilder _chainBuilder;

    /// <summary>
    /// Service provider for creating scoped instances
    /// </summary>
    private readonly IServiceProvider _provider;

    /// <summary>
    /// Cache for storing built execution chains by input/output type pairs
    /// </summary>
    private readonly IDictionary<ValueTuple<Type, Type>, IReadOnlyList<ChainElement>> _chainCache =
        new Dictionary<ValueTuple<Type, Type>, IReadOnlyList<ChainElement>>();

    /// <summary>
    /// Initializes a new instance of the Mediator class
    /// </summary>
    /// <param name="chainBuilder">Builder for creating execution chains</param>
    /// <param name="provider">Service provider for dependency injection</param>
    public Mediator(ChainBuilder chainBuilder, IServiceProvider provider)
    {
        _chainBuilder = chainBuilder;
        _provider = provider;
    }

    /// <summary>
    /// Sends a request and returns the response using a scoped service provider
    /// </summary>
    /// <typeparam name="TResponse">Expected response type</typeparam>
    /// <param name="request">Request object to process</param>
    /// <param name="ct">Cancellation token for the operation</param>
    /// <returns>Response of the specified type</returns>
    public async Task<TResponse> SendAsync<TResponse>(object request, CancellationToken ct = default)
    {
        // get execution chain with last item, being final one
        var chain = GetChain(request.GetType(), typeof(TResponse));

        // use scoped service provider
        await using var scope = _provider.CreateAsyncScope();
        return (TResponse)await ChainExecutor.ExecuteAsync(scope.ServiceProvider, chain, request, ct);
    }

    /// <summary>
    /// Sends a request and returns the response using the provided service provider
    /// </summary>
    /// <typeparam name="TResponse">Expected response type</typeparam>
    /// <param name="serviceProvider">Service provider to use for resolving dependencies</param>
    /// <param name="request">Request object to process</param>
    /// <param name="ct">Cancellation token for the operation</param>
    /// <returns>Response of the specified type</returns>
    public async Task<TResponse> SendAsync<TResponse>(
        IServiceProvider serviceProvider,
        object request,
        CancellationToken ct = default
    )
    {
        // get execution chain with last item, being final one
        var chain = GetChain(request.GetType(), typeof(TResponse));

        // use given service provider
        return (TResponse)await ChainExecutor.ExecuteAsync(serviceProvider, chain, request, ct);
    }

    /// <summary>
    /// Gets or builds an execution chain for the specified input and output types
    /// </summary>
    /// <param name="input">Input request type</param>
    /// <param name="output">Expected output response type</param>
    /// <returns>Execution chain for processing the request</returns>
    private IReadOnlyList<ChainElement> GetChain(Type input, Type output)
    {
        lock (_chainCache)
        {
            var key = (input, output);
            if (_chainCache.TryGetValue(key, out var chain))
                return chain;

            return _chainCache[key] = _chainBuilder.BuildExecutionChain(input, output);
        }
    }
}
