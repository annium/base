using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Base interface for bulk registration builder.
/// </summary>
public interface IBulkRegistrationBuilderBase : IBulkRegistrationBuilderTarget
{
    /// <summary>
    /// Filter types for registration
    /// </summary>
    /// <param name="predicate">type filter</param>
    /// <returns>builder with applied filter</returns>
    IBulkRegistrationBuilderBase Where(Func<Type, bool> predicate);
}

/// <summary>
/// Interface for bulk registration builder target operations.
/// </summary>
public interface IBulkRegistrationBuilderTarget : IBulkRegistrationBuilderLifetime
{
    /// <summary>
    /// Register all types as themselves
    /// </summary>
    /// <returns>target builder</returns>
    IBulkRegistrationBuilderTarget AsSelf();

    /// <summary>
    /// Register all types as given service type
    /// </summary>
    /// <param name="serviceType">service type</param>
    /// <returns>target builder</returns>
    IBulkRegistrationBuilderTarget As(Type serviceType);

    /// <summary>
    /// Register all types as each of their interfaces
    /// </summary>
    /// <returns>target builder</returns>
    IBulkRegistrationBuilderTarget AsInterfaces();

    /// <summary>
    /// Register all types as themselves with key resolution function
    /// </summary>
    /// <param name="getKey">Function to get key for each type</param>
    /// <returns>target builder</returns>
    IBulkRegistrationBuilderTarget AsKeyedSelf(Func<Type, object> getKey);

    /// <summary>
    /// Register all types as given service type with key resolution function
    /// </summary>
    /// <param name="serviceType">service type</param>
    /// <param name="getKey">Function to get key for each type</param>
    /// <returns>target builder</returns>
    IBulkRegistrationBuilderTarget AsKeyed(Type serviceType, Func<Type, object> getKey);

    /// <summary>
    /// Register all types as self factories
    /// </summary>
    /// <returns>target builder</returns>
    IBulkRegistrationBuilderTarget AsSelfFactory();

    /// <summary>
    /// Register all types as factories of given service type
    /// </summary>
    /// <param name="serviceType">service type</param>
    /// <returns>target builder</returns>
    IBulkRegistrationBuilderTarget AsFactory(Type serviceType);

    /// <summary>
    /// Register all types as self factories with key resolution function
    /// </summary>
    /// <param name="getKey">Function to get key for each type</param>
    /// <returns>target builder</returns>
    IBulkRegistrationBuilderTarget AsKeyedSelfFactory(Func<Type, object> getKey);

    /// <summary>
    /// Register all types as factories of given service type with key resolution function
    /// </summary>
    /// <param name="serviceType">service type</param>
    /// <param name="getKey">Function to get key for each type</param>
    /// <returns>target builder</returns>
    IBulkRegistrationBuilderTarget AsKeyedFactory(Type serviceType, Func<Type, object> getKey);
}

/// <summary>
/// Interface for bulk registration builder lifetime operations.
/// </summary>
public interface IBulkRegistrationBuilderLifetime
{
    /// <summary>
    /// Sets the service lifetime for all registrations.
    /// </summary>
    /// <param name="lifetime">The service lifetime to use</param>
    /// <returns>The service container instance</returns>
    IServiceContainer In(ServiceLifetime lifetime);

    /// <summary>
    /// Sets the service lifetime to scoped for all registrations.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Scoped();

    /// <summary>
    /// Sets the service lifetime to singleton for all registrations.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Singleton();

    /// <summary>
    /// Sets the service lifetime to transient for all registrations.
    /// </summary>
    /// <returns>The service container instance</returns>
    IServiceContainer Transient();
}
