using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.Core.Mediator.Internal;

internal class Mediator : IMediator
{
    private readonly ChainBuilder _chainBuilder;
    private readonly IServiceProvider _provider;

    private readonly IDictionary<ValueTuple<Type, Type>, IReadOnlyList<ChainElement>> _chainCache =
        new Dictionary<ValueTuple<Type, Type>, IReadOnlyList<ChainElement>>();

    public Mediator(
        ChainBuilder chainBuilder,
        IServiceProvider provider
    )
    {
        _chainBuilder = chainBuilder;
        _provider = provider;
    }

    public async Task<TResponse> SendAsync<TResponse>(
        object request,
        CancellationToken ct = default
    )
    {
        // get execution chain with last item, being final one
        var chain = GetChain(request.GetType(), typeof(TResponse));

        // use scoped service provider
        await using var scope = _provider.CreateAsyncScope();
        return (TResponse)await ChainExecutor.ExecuteAsync(scope.ServiceProvider, chain, request, ct);
    }

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