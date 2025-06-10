namespace Annium.Core.DependencyInjection.Descriptors;

/// <summary>
/// Service descriptor that uses a pre-created instance for a keyed service
/// </summary>
public interface IKeyedInstanceServiceDescriptor : IServiceDescriptor
{
    /// <summary>
    /// Pre-created instance of the keyed service
    /// </summary>
    object ImplementationInstance { get; }
}
