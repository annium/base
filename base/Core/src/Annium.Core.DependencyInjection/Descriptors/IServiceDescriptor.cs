using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IServiceDescriptor
{
    public Type ServiceType { get; }
    public object? Key { get; }
    public ServiceLifetime Lifetime { get; }
}
