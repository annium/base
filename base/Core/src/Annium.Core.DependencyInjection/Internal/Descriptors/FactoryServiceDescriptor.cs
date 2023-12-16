using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

internal sealed record FactoryServiceDescriptor : IFactoryServiceDescriptor
{
    public required ServiceLifetime Lifetime { get; init; }
    public required Type ServiceType { get; init; }
    public required Func<IServiceProvider, object> ImplementationFactory { get; init; }
}
