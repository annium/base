using System;
using Annium.Reflection;
using IBuilderBase = Annium.Core.DependencyInjection.IBulkRegistrationBuilderBase;
using IBuilderTarget = Annium.Core.DependencyInjection.IBulkRegistrationBuilderTarget;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class BulkRegistrationBuilderExtensions
{
    public static IBuilderBase AssignableTo<T>(this IBuilderBase builder)
        => builder.Where(x => x.IsDerivedFrom(typeof(T)));

    public static IBuilderBase AssignableTo(this IBuilderBase builder, Type baseType)
        => builder.Where(x => x.IsDerivedFrom(baseType));

    public static IBuilderBase StartingWith(this IBuilderBase builder, string prefix)
        => builder.Where(x => x.Name.StartsWith(prefix));

    public static IBuilderBase EndingWith(this IBuilderBase builder, string suffix)
        => builder.Where(x => x.Name.EndsWith(suffix));

    public static IBuilderTarget As<T>(this IBuilderTarget builder) =>
        builder.As(typeof(T));

    public static IBuilderTarget AsKeyed<T, TKey>(this IBuilderTarget builder, Func<Type, TKey> getKey) where TKey : notnull =>
        builder.AsKeyed(typeof(T), getKey);

    public static IBuilderTarget AsFactory<T>(this IBuilderTarget builder) =>
        builder.AsFactory(typeof(T));

    public static IBuilderTarget AsKeyedFactory<T, TKey>(this IBuilderTarget builder, Func<Type, TKey> getKey) where TKey : notnull =>
        builder.AsKeyedFactory(typeof(T), getKey);
}