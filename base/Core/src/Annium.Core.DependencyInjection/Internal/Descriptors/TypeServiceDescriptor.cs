using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

internal sealed record TypeServiceDescriptor : ITypeServiceDescriptor
{
    public required Type ServiceType { get; init; }
    public object? Key => null;
    public required Type ImplementationType { get; init; }
    public required ServiceLifetime Lifetime { get; init; }
}
