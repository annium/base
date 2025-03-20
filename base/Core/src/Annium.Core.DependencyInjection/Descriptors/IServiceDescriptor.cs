using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IServiceDescriptor
{
    Type ServiceType { get; }
    object? Key { get; }
    ServiceLifetime Lifetime { get; }
}
