// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IInstanceServiceDescriptor : IServiceDescriptor
{
    public object ImplementationInstance { get; }
}
