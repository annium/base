using IBuilderTarget = Annium.Core.DependencyInjection.ISingleRegistrationBuilderBase;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class SingleRegistrationBuilderExtensions
{
    public static IBuilderTarget As<T>(this IBuilderTarget builder) => builder.As(typeof(T));

    public static IBuilderTarget AsKeyed<T>(this IBuilderTarget builder, object key) => builder.AsKeyed(typeof(T), key);

    public static IBuilderTarget AsFactory<T>(this IBuilderTarget builder) => builder.AsFactory(typeof(T));

    public static IBuilderTarget AsKeyedFactory<T>(this IBuilderTarget builder, object key) =>
        builder.AsKeyedFactory(typeof(T), key);
}
