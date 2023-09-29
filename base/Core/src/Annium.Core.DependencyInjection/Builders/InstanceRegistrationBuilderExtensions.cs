using IBuilderTarget = Annium.Core.DependencyInjection.IInstanceRegistrationBuilderBase;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class InstanceRegistrationBuilderExtensions
{
    public static IBuilderTarget As<T>(this IBuilderTarget builder) =>
        builder.As(typeof(T));

    public static IBuilderTarget AsKeyed<T, TKey>(this IBuilderTarget builder, TKey key) where TKey : notnull =>
        builder.AsKeyed(typeof(T), key);

    public static IBuilderTarget AsFactory<T>(this IBuilderTarget builder) =>
        builder.AsFactory(typeof(T));

    public static IBuilderTarget AsKeyedFactory<T, TKey>(this IBuilderTarget builder, TKey key) where TKey : notnull =>
        builder.AsKeyedFactory(typeof(T), key);
}