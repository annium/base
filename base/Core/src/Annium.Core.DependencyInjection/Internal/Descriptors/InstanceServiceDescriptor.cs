using System;

namespace Annium.Core.DependencyInjection.Internal.Descriptors;

internal sealed record InstanceServiceDescriptor : IInstanceServiceDescriptor
{
    public required Type ServiceType { get; init; }
    public object? Key => null;
    public required object ImplementationInstance { get; init; }
    public required ServiceLifetime Lifetime { get; init; }
}
