using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/*
 * Registration types: (S - service type, IT - impl type, IF - impl factory, II - impl instance, K - key type, SP - IServiceProvider)
 * - Type as:                   S = S or S = SP => Resolve(IT)
 * - Type as keyed:             KV<K,S> = SP => new KV<K,S>(key, Resolve(IT))
 * - Type as factory:           Func<S> = SP => () => Resolve(IT)
 * - Type as keyed factory:     KV<K,Func<S>> = SP => new KV<K,Func<S>>(key, () => Resolve(IT))
 * - Factory as:                S = IF
 * - Factory as keyed:          KV<K,S> = SP => new KV<K,S>(key, IF(SP))
 * - Instance as:               S = II
 * - Instance as keyed:         KV<K,S> = SP => new KV<K,S>(key, II)
 * - Instance as factory:       Func<S> = SP => () => II
 * - Instance as keyed factory: KV<K,Func<S>> = SP => new KV<K,Func<S>>(key, () => II)
 *
 * Helpers:
 * Service:
 * - S | Func<S>
 * - KV<K,Service>
 * Factories:
 * - Resolve(IT) | () => Resolve(IT) | IF(SP) | II | () => II
 * - new KV<K,S>(key, Implementation)
 * Samples:
 * - S = SP => Resolve(IT)
 * - KV<K,S> = SP => new KV<K,S>(key, Resolve(IT))
 * - Func<S> = SP => () => Resolve(IT)
 * - KV<K,Func<S>> = SP => new KV<K,Func<S>>(key, () => Resolve(IT))
 * - KV<K,S> = SP => new KV<K,S>(key, IF(SP))
 * - KV<K,S> = SP => new KV<K,S>(key, II)
 * - Func<S> = SP => () => II
 * - KV<K,Func<S>> = SP => new KV<K,Func<S>>(key, () => II)
 */
internal static class Helper
{
    public static Type FactoryType(Type type) => typeof(Func<>).MakeGenericType(type);

    public static Type KeyValueType(Type keyType, Type valueType) => typeof(KeyValue<,>).MakeGenericType(keyType, valueType);

    public static IServiceDescriptor Factory(Type serviceType, Func<ParameterExpression, Expression> getBody, ServiceLifetime lifetime)
    {
        var sp = Expression.Parameter(typeof(IServiceProvider));
        var body = getBody(sp);
        var function = Expression.Lambda(body, sp).Compile();

        return ServiceDescriptor.Factory(serviceType, (Func<IServiceProvider, object>)function, lifetime);
    }

    public static Expression KeyValue(Type keyType, Type valueType, object key, Expression value) =>
        Expression.New(
            KeyValueConstructor(keyType, valueType),
            Expression.Constant(key),
            Expression.Lambda(value)
        );

    public static Expression Resolve(ParameterExpression sp, Type type)
    {
        var getRequiredService = GetRequiredService(type);

        return Expression.Call(null, getRequiredService, sp);
    }

    private static readonly MethodInfo GetRequiredServiceMethod = typeof(ServiceProviderServiceExtensions)
        .GetMethods()
        .Single(x =>
            x.Name == nameof(ServiceProviderServiceExtensions.GetRequiredService) &&
            x.GetParameters().Length == 1 &&
            x.GetParameters().Single().ParameterType == typeof(IServiceProvider)
        );

    private static MethodInfo GetRequiredService(Type type) => GetRequiredServiceMethod.MakeGenericMethod(type);

    private static ConstructorInfo KeyValueConstructor(Type keyType, Type valueType)
    {
        var type = typeof(KeyValue<,>).MakeGenericType(keyType, valueType);

        return type.GetConstructor(new[] { keyType, typeof(Func<>).MakeGenericType(valueType) })
            ?? throw new MissingMethodException($"Failed to find {type.FriendlyName()} constructor");
    }
}