using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IKeyedTypeServiceDescriptor : IServiceDescriptor
{
    Type ImplementationType { get; }
}
