using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/// <summary>
/// Represents a service registration that can be resolved to service descriptors
/// </summary>
internal interface IRegistration
{
    /// <summary>
    /// Resolves this registration into service descriptors with the specified lifetime
    /// </summary>
    /// <param name="lifetime">The service lifetime to apply</param>
    /// <returns>The collection of service descriptors</returns>
    IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime);
}
