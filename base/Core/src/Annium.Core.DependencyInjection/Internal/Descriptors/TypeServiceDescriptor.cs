using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

internal sealed record TypeServiceDescriptor : ITypeServiceDescriptor
{
    public required ServiceLifetime Lifetime { get; init; }
    public required Type ServiceType { get; init; }
    public required Type ImplementationType { get; init; }
}
