using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal.Container;

internal class AsyncServiceScope : IAsyncServiceScope
{
    public IServiceProvider ServiceProvider => _scope.ServiceProvider;

    private readonly IServiceScope _scope;

    public AsyncServiceScope(IServiceScope scope)
    {
        _scope = scope;
    }

    public ValueTask DisposeAsync() => _scope.DisposeAsync();
}