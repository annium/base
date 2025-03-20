// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IKeyedInstanceServiceDescriptor : IServiceDescriptor
{
    object ImplementationInstance { get; }
}
