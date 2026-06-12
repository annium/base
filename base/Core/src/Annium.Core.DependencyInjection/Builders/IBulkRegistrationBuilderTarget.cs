using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Interface for bulk registration builder target operations.
/// </summary>
public interface IBulkRegistrationBuilderTarget : IRegistrationBuilderLifetime
{
    /// <summary>
    /// Registers all types as themselves.
    /// </summary>
    /// <returns>Target builder.</returns>
    IBulkRegistrationBuilderTarget AsSelf();

    /// <summary>
    /// Registers all types as the given service type.
    /// </summary>
    /// <param name="serviceType">Service type.</param>
    /// <returns>Target builder.</returns>
    IBulkRegistrationBuilderTarget As(Type serviceType);

    /// <summary>
    /// Registers each type as each of its interfaces.
    /// </summary>
    /// <returns>Target builder.</returns>
    IBulkRegistrationBuilderTarget AsInterfaces();

    /// <summary>
    /// Registers all types as themselves with a key resolution function.
    /// </summary>
    /// <param name="getKey">Function to compute the key for each type.</param>
    /// <returns>Target builder.</returns>
    IBulkRegistrationBuilderTarget AsKeyedSelf(Func<Type, object> getKey);

    /// <summary>
    /// Registers all types as the given service type with a key resolution function.
    /// </summary>
    /// <param name="serviceType">Service type.</param>
    /// <param name="getKey">Function to compute the key for each type.</param>
    /// <returns>Target builder.</returns>
    IBulkRegistrationBuilderTarget AsKeyed(Type serviceType, Func<Type, object> getKey);

    /// <summary>
    /// Registers each type as each of its implemented interfaces using a single key per type
    /// (every interface registration of that type shares the same key).
    /// </summary>
    /// <param name="getKey">Function to compute the key for each implementation type — applied once per type and reused across that type's interface registrations.</param>
    /// <returns>Target builder.</returns>
    IBulkRegistrationBuilderTarget AsKeyedInterfaces(Func<Type, object> getKey);

    /// <summary>
    /// Registers all types as self factories.
    /// </summary>
    /// <returns>Target builder.</returns>
    IBulkRegistrationBuilderTarget AsSelfFactory();

    /// <summary>
    /// Registers all types as factories of the given service type.
    /// </summary>
    /// <param name="serviceType">Service type.</param>
    /// <returns>Target builder.</returns>
    IBulkRegistrationBuilderTarget AsFactory(Type serviceType);

    /// <summary>
    /// Registers all types as self factories with a key resolution function.
    /// </summary>
    /// <param name="getKey">Function to compute the key for each type.</param>
    /// <returns>Target builder.</returns>
    IBulkRegistrationBuilderTarget AsKeyedSelfFactory(Func<Type, object> getKey);

    /// <summary>
    /// Registers all types as factories of the given service type with a key resolution function.
    /// </summary>
    /// <param name="serviceType">Service type.</param>
    /// <param name="getKey">Function to compute the key for each type.</param>
    /// <returns>Target builder.</returns>
    IBulkRegistrationBuilderTarget AsKeyedFactory(Type serviceType, Func<Type, object> getKey);
}
