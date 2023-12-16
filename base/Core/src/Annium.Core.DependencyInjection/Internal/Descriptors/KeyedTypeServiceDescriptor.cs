using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

internal sealed record KeyedTypeServiceDescriptor : IKeyedTypeServiceDescriptor
{
    public required ServiceLifetime Lifetime { get; init; }
    public required Type ServiceType { get; init; }
    public required object Key { get; init; }
    public required Type ImplementationType { get; init; }
}
