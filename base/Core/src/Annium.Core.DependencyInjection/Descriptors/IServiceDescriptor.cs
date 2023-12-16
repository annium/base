using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IServiceDescriptor
{
    public ServiceLifetime Lifetime { get; }

    public Type ServiceType { get; }
}
