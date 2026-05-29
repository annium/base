namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Service descriptor for keyed instance-based services
/// </summary>
internal sealed record KeyedInstanceServiceDescriptor : KeyedServiceDescriptorBase, IKeyedInstanceServiceDescriptor
{
    /// <summary>
    /// Gets the service instance
    /// </summary>
    public required object ImplementationInstance { get; init; }
}
