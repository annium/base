using IBuilderBase = Annium.Core.DependencyInjection.IFactoryRegistrationBuilderBase;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class FactoryRegistrationBuilderExtensions
{
    public static IBuilderBase As<T>(this IBuilderBase builder) => builder.As(typeof(T));
}
