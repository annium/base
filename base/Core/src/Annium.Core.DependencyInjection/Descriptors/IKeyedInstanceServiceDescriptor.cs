// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IKeyedInstanceServiceDescriptor : IKeyedServiceDescriptor
{
    public object ImplementationInstance { get; }
}
