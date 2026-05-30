using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper.Internal;
using Annium.Reflection;

namespace Annium.Core.Mapper;

/// <summary>
/// Extensions for configuring mapper services in the service container.
/// </summary>
/// <remarks>
/// The bulk of the registration is encapsulated in <see cref="MapperRegistration"/>; this file
/// keeps the consumer-facing fluent API stable while co-locating the wiring in one helper.
/// </remarks>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds mapper services to the service container.
    /// </summary>
    /// <param name="container">The service container.</param>
    /// <param name="autoload">Whether to autoload profiles via the registered type manager.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddMapper(this IServiceContainer container, bool autoload = true)
    {
        MapperRegistration.Configure(container, autoload);
        return container;
    }

    /// <summary>
    /// Adds a configured profile to the service container.
    /// </summary>
    /// <param name="container">The service container.</param>
    /// <param name="configure">The profile configuration action.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddProfile(this IServiceContainer container, Action<Profile> configure)
    {
        var profile = new EmptyProfile();
        configure(profile);

        MapperRegistration.AddProfileInstance(container, profile);

        return container;
    }

    /// <summary>
    /// Adds a profile type to the service container.
    /// </summary>
    /// <typeparam name="T">The profile type.</typeparam>
    /// <param name="container">The service container.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddProfile<T>(this IServiceContainer container)
        where T : Profile
    {
        MapperRegistration.AddProfileType(container, typeof(T));

        return container;
    }

    /// <summary>
    /// Adds a profile type to the service container.
    /// </summary>
    /// <param name="container">The service container.</param>
    /// <param name="profileType">The profile type.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddProfile(this IServiceContainer container, Type profileType)
    {
        if (!profileType.GetInheritanceChain().Contains(typeof(Profile)))
            throw new ArgumentException($"Type {profileType} is not inherited from {typeof(Profile)}");

        MapperRegistration.AddProfileType(container, profileType);

        return container;
    }
}
