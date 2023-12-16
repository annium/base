using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

internal sealed record InstanceServiceDescriptor : IInstanceServiceDescriptor
{
    public required ServiceLifetime Lifetime { get; init; }
    public required Type ServiceType { get; init; }
    public required object ImplementationInstance { get; init; }
}
