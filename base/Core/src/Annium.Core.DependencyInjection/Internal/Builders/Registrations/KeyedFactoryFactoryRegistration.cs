using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/// <summary>
/// Registration that wraps a user keyed factory in a keyed <c>Func&lt;T&gt;</c> descriptor. Each
/// call to the resolved <c>Func&lt;T&gt;</c> invokes the original keyed factory with the resolving
/// <see cref="IServiceProvider"/> and the matched key.
/// </summary>
internal class KeyedFactoryFactoryRegistration : IRegistration
{
    /// <summary>The service type to be wrapped as <c>Func&lt;T&gt;</c>.</summary>
    private readonly Type _serviceType;

    /// <summary>The key associated with this keyed factory registration.</summary>
    private readonly object _key;

    /// <summary>The user-supplied keyed factory delegate to invoke on each <c>Func&lt;T&gt;</c> call.</summary>
    private readonly Func<IServiceProvider, object, object> _factory;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="serviceType">The service type to be wrapped as <c>Func&lt;T&gt;</c>.</param>
    /// <param name="key">The key associated with the registration.</param>
    /// <param name="factory">The user-supplied keyed factory delegate.</param>
    public KeyedFactoryFactoryRegistration(Type serviceType, object key, Func<IServiceProvider, object, object> factory)
    {
        _serviceType = serviceType;
        _key = key;
        _factory = factory;
    }

    /// <summary>
    /// Resolves this registration into a single keyed <c>Func&lt;T&gt;</c> service descriptor.
    /// </summary>
    /// <param name="lifetime">The service lifetime to apply to the descriptor.</param>
    /// <returns>Descriptors for the keyed <c>Func&lt;T&gt;</c> registration.</returns>
    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            FactoryType(_serviceType),
            _key,
            (sp, key) =>
                Expression.Lambda(
                    Expression.Convert(Expression.Invoke(Expression.Constant(_factory), sp, key), _serviceType)
                ),
            lifetime
        );
    }
}
