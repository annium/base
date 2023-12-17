using IBuilderBase = Annium.Core.DependencyInjection.IKeyedFactoryRegistrationBuilderBase;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class KeyedFactoryRegistrationBuilderExtensions
{
    public static IBuilderBase AsKeyed<T>(this IBuilderBase builder, object key) => builder.AsKeyed(typeof(T), key);
}
