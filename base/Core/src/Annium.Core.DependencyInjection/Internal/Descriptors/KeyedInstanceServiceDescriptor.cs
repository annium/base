using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

internal sealed record KeyedInstanceServiceDescriptor : IKeyedInstanceServiceDescriptor
{
    public required Type ServiceType { get; init; }
    public required object Key { get; init; }
    public required object ImplementationInstance { get; init; }
    public required ServiceLifetime Lifetime { get; init; }
}
