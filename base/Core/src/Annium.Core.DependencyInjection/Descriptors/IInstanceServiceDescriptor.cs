// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public interface IInstanceServiceDescriptor : IServiceDescriptor
{
    object ImplementationInstance { get; }
}
