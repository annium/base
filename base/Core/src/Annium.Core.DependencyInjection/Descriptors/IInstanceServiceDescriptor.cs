// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Service descriptor that uses a pre-created instance for the service
/// </summary>
public interface IInstanceServiceDescriptor : IServiceDescriptor
{
    /// <summary>
    /// Pre-created instance of the service
    /// </summary>
    object ImplementationInstance { get; }
}
