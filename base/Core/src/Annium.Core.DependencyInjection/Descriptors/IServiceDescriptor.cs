using System;

namespace Annium.Core.DependencyInjection.Descriptors;

/// <summary>
/// Base interface for service descriptors that define how services are registered and resolved
/// </summary>
public interface IServiceDescriptor
{
    /// <summary>
    /// Type of the service being described
    /// </summary>
    Type ServiceType { get; }

    /// <summary>
    /// Optional key for keyed services
    /// </summary>
    object? Key { get; }

    /// <summary>
    /// Lifetime management for the service
    /// </summary>
    ServiceLifetime Lifetime { get; }
}
