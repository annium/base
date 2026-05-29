namespace Annium.Core.DependencyInjection.Internal.Descriptors;

/// <summary>
/// Service descriptor for instance-based services
/// </summary>
internal sealed record InstanceServiceDescriptor : NonKeyedServiceDescriptorBase, IInstanceServiceDescriptor
{
    /// <summary>
    /// Gets the service instance
    /// </summary>
    public required object ImplementationInstance { get; init; }
}
