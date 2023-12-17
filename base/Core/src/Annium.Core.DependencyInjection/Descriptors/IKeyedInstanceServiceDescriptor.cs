// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IKeyedInstanceServiceDescriptor : IServiceDescriptor
{
    public object ImplementationInstance { get; }
}
