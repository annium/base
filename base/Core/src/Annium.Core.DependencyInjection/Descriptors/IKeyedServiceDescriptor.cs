// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IKeyedServiceDescriptor : IServiceDescriptor
{
    public object Key { get; }
}
