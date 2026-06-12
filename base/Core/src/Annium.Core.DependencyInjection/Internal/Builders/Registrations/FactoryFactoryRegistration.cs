using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations;

/// <summary>
/// Registration that wraps a user factory in a <c>Func&lt;T&gt;</c> descriptor. Each call to the
/// resolved <c>Func&lt;T&gt;</c> invokes the original factory with the resolving
/// <see cref="IServiceProvider"/>, allowing consumers to defer resolution while still going
/// through the user-supplied factory logic.
/// </summary>
internal class FactoryFactoryRegistration : IRegistration
{
    /// <summary>The service type to be wrapped as <c>Func&lt;T&gt;</c>.</summary>
    private readonly Type _serviceType;

    /// <summary>The user-supplied factory delegate to invoke on each <c>Func&lt;T&gt;</c> call.</summary>
    private readonly Func<IServiceProvider, object> _factory;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="serviceType">The service type to be wrapped as <c>Func&lt;T&gt;</c>.</param>
    /// <param name="factory">The user-supplied factory delegate.</param>
    public FactoryFactoryRegistration(Type serviceType, Func<IServiceProvider, object> factory)
    {
        _serviceType = serviceType;
        _factory = factory;
    }

    /// <summary>
    /// Resolves this registration into a single <c>Func&lt;T&gt;</c> service descriptor.
    /// </summary>
    /// <param name="lifetime">The service lifetime to apply to the descriptor.</param>
    /// <returns>Descriptors for the <c>Func&lt;T&gt;</c> registration.</returns>
    public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
    {
        yield return Factory(
            FactoryType(_serviceType),
            sp =>
                Expression.Lambda(
                    Expression.Convert(Expression.Invoke(Expression.Constant(_factory), sp), _serviceType)
                ),
            lifetime
        );
    }
}
