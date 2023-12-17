using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

internal sealed record FactoryServiceDescriptor : IFactoryServiceDescriptor
{
    public required Type ServiceType { get; init; }
    public object? Key => null;
    public required Func<IServiceProvider, object> ImplementationFactory { get; init; }
    public required ServiceLifetime Lifetime { get; init; }
}
