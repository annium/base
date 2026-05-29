using System;
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
/// <summary>
/// Helper class for creating service descriptors and expression trees for dependency injection registrations
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Cached method info for the GetRequiredService&lt;T&gt;(IServiceProvider) overload.
    /// Looked up by exact signature (1 generic parameter, IServiceProvider parameter) so that
    /// future overloads with the same name don't break the lookup.
    /// </summary>
    private static readonly MethodInfo _getRequiredServiceMethod =
        typeof(ServiceProviderServiceExtensions).GetMethod(
            nameof(ServiceProviderServiceExtensions.GetRequiredService),
            genericParameterCount: 1,
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(IServiceProvider)],
            modifiers: null
        )
        ?? throw new MissingMethodException(
            $"{nameof(ServiceProviderServiceExtensions)}.{nameof(ServiceProviderServiceExtensions.GetRequiredService)}<T>(IServiceProvider) not found"
        );

    /// <summary>
    /// Cached method info for the GetRequiredKeyedService&lt;T&gt;(IServiceProvider, object?) overload.
    /// Looked up by exact signature (1 generic parameter, IServiceProvider + object parameters) so
    /// that future overloads with the same name don't break the lookup.
    /// </summary>
    private static readonly MethodInfo _getRequiredKeyedServiceMethod =
        typeof(ServiceProviderKeyedServiceExtensions).GetMethod(
            nameof(ServiceProviderKeyedServiceExtensions.GetRequiredKeyedService),
            genericParameterCount: 1,
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: [typeof(IServiceProvider), typeof(object)],
            modifiers: null
        )
        ?? throw new MissingMethodException(
            $"{nameof(ServiceProviderKeyedServiceExtensions)}.{nameof(ServiceProviderKeyedServiceExtensions.GetRequiredKeyedService)}<T>(IServiceProvider, object) not found"
        );

    /// <summary>
    /// Creates a factory type for the specified type
    /// </summary>
    /// <param name="type">The type to create a factory for</param>
    /// <returns>The factory type</returns>
    public static Type FactoryType(Type type) => typeof(Func<>).MakeGenericType(type);

    /// <summary>
    /// Creates a service descriptor with a factory using an expression body
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="getBody">Function to generate the expression body</param>
    /// <param name="lifetime">The service lifetime</param>
    /// <returns>The service descriptor</returns>
    public static IServiceDescriptor Factory(
        Type serviceType,
        Func<ParameterExpression, Expression> getBody,
        ServiceLifetime lifetime
    )
    {
        var spEx = Expression.Parameter(typeof(IServiceProvider));
        var body = getBody(spEx);
        var function = Expression.Lambda(body, spEx).Compile();

        return ServiceDescriptor.Factory(serviceType, (Func<IServiceProvider, object>)function, lifetime);
    }

    /// <summary>
    /// Creates a keyed service descriptor with a factory using an expression body
    /// </summary>
    /// <param name="serviceType">The service type</param>
    /// <param name="key">The service key</param>
    /// <param name="getBody">Function to generate the expression body</param>
    /// <param name="lifetime">The service lifetime</param>
    /// <returns>The service descriptor</returns>
    public static IServiceDescriptor Factory(
        Type serviceType,
        object key,
        Func<ParameterExpression, ParameterExpression, Expression> getBody,
        ServiceLifetime lifetime
    )
    {
        var spEx = Expression.Parameter(typeof(IServiceProvider));
        var keyEx = Expression.Parameter(typeof(object));
        var body = getBody(spEx, keyEx);
        var function = Expression.Lambda(body, spEx, keyEx).Compile();

        return ServiceDescriptor.KeyedFactory(
            serviceType,
            key,
            (Func<IServiceProvider, object, object>)function,
            lifetime
        );
    }

    /// <summary>
    /// Creates an expression for resolving a service from a service provider
    /// </summary>
    /// <param name="sp">The service provider parameter expression</param>
    /// <param name="type">The type to resolve</param>
    /// <returns>The resolve expression</returns>
    public static Expression Resolve(ParameterExpression sp, Type type)
    {
        var getRequiredService = GetRequiredService(type);

        return Expression.Call(null, getRequiredService, sp);
    }

    /// <summary>
    /// Creates an expression for resolving a keyed service from a service provider
    /// </summary>
    /// <param name="sp">The service provider parameter expression</param>
    /// <param name="key">The key parameter expression</param>
    /// <param name="type">The type to resolve</param>
    /// <returns>The resolve expression</returns>
    public static Expression Resolve(ParameterExpression sp, ParameterExpression key, Type type)
    {
        var getRequiredService = GetRequiredKeyedService(type);

        return Expression.Call(null, getRequiredService, sp, key);
    }

    /// <summary>
    /// Gets the generic GetRequiredService method for the specified type
    /// </summary>
    /// <param name="type">The service type</param>
    /// <returns>The generic method info</returns>
    private static MethodInfo GetRequiredService(Type type) => _getRequiredServiceMethod.MakeGenericMethod(type);

    /// <summary>
    /// Gets the generic GetRequiredKeyedService method for the specified type
    /// </summary>
    /// <param name="type">The service type</param>
    /// <returns>The generic method info</returns>
    private static MethodInfo GetRequiredKeyedService(Type type) =>
        _getRequiredKeyedServiceMethod.MakeGenericMethod(type);
}
