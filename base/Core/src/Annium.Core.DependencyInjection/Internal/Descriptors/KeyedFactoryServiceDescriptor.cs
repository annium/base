using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

internal sealed record KeyedFactoryServiceDescriptor : IKeyedFactoryServiceDescriptor
{
    public required Type ServiceType { get; init; }
    public required object Key { get; init; }
    public required Func<IServiceProvider, object, object> ImplementationFactory { get; init; }
    public required ServiceLifetime Lifetime { get; init; }
}
