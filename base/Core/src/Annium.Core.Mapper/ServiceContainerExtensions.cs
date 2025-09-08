using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper.Attributes;
using Annium.Core.Mapper.Internal;
using Annium.Core.Mapper.Internal.DependencyInjection;
using Annium.Core.Mapper.Internal.Profiles;
using Annium.Core.Mapper.Internal.Resolvers;
using Annium.Core.Runtime;
using Annium.Core.Runtime.Types;
using Annium.Logging;
using Annium.Reflection;

namespace Annium.Core.Mapper;

/// <summary>
/// Extensions for configuring mapper services in the service container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds mapper services to the service container
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="autoload">Whether to automatically discover and register profiles</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddMapper(this IServiceContainer container, bool autoload = true)
    {
        // register base services
        container.Add<IRepacker, Repacker>().Singleton();
        container.Add<IMapBuilder, MapBuilder>().Singleton();
        container.Add<IMapper, Internal.Mapper>().Singleton();
        container
            .Add(sp => new Lazy<IMapContext>(() => new MapContext(sp.Resolve<IMapper>()), true))
            .AsSelf()
            .Singleton();

        // register resolvers
        container.Add<IMapResolver, InstanceOfMapResolver>().Singleton();
        container.Add<IMapResolver, EnumerableMapResolver>().Singleton();
        container.Add<IMapResolver, ResolutionMapResolver>().Singleton();
        container.Add<IMapResolver, DictionaryConstructorMapResolver>().Singleton();
        container.Add<IMapResolver, ConstructorMapResolver>().Singleton();
        container.Add<IMapResolver, DictionaryAssignmentMapResolver>().Singleton();
        container.Add<IMapResolver, AssignmentMapResolver>().Singleton();

        // add default profile
        container.AddProfileInstance(new EmptyProfile());
        container.AddProfileInstance(new DefaultProfile());
        container.AddProfileType(typeof(EnumProfile<>));
        container.AddProfileType(typeof(EnumProfile<,>));

        // special cases
        container.AddProfileInstance(new EnumProfile<LogLevel>());

        // if autoload requested - discover and register profiles
        if (autoload)
        {
            var typeManager = container.GetTypeManager();

            foreach (var profileType in typeManager.GetImplementations(typeof(Profile)))
                container.AddProfileType(profileType);
        }

        // register profile resolution
        container.Add(ResolveProfiles).AsSelf().Singleton();

        return container;
    }

    /// <summary>
    /// Adds a configured profile to the service container
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="configure">The profile configuration action</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddProfile(this IServiceContainer container, Action<Profile> configure)
    {
        var profile = new EmptyProfile();
        configure(profile);

        container.Add(new ProfileInstance(profile)).AsSelf().Singleton();

        return container;
    }

    /// <summary>
    /// Adds a profile type to the service container
    /// </summary>
    /// <typeparam name="T">The profile type</typeparam>
    /// <param name="container">The service container</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddProfile<T>(this IServiceContainer container)
        where T : Profile
    {
        container.AddProfileType(typeof(T));

        return container;
    }

    /// <summary>
    /// Adds a profile type to the service container
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="profileType">The profile type</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddProfile(this IServiceContainer container, Type profileType)
    {
        if (!profileType.GetInheritanceChain().Contains(typeof(Profile)))
            throw new ArgumentException($"Type {profileType} is not inherited from {typeof(Profile)}");

        container.AddProfileType(profileType);

        return container;
    }

    /// <summary>
    /// Adds a profile instance to the service container
    /// </summary>
    /// <typeparam name="T">The profile type</typeparam>
    /// <param name="container">The service container</param>
    /// <param name="profile">The profile instance</param>
    /// <returns>The service container for method chaining</returns>
    private static IServiceContainer AddProfileInstance<T>(this IServiceContainer container, T profile)
        where T : Profile
    {
        container.Add(profile).AsSelf().Singleton();
        container.Add(new ProfileInstance(profile)).AsSelf().Singleton();

        return container;
    }

    /// <summary>
    /// Adds a profile type to the service container
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="profileType">The profile type</param>
    /// <returns>The service container for method chaining</returns>
    private static IServiceContainer AddProfileType(this IServiceContainer container, Type profileType)
    {
        container.Add(profileType).AsSelf().Singleton();
        container.Add(new ProfileType(profileType)).AsSelf().Singleton();

        return container;
    }

    /// <summary>
    /// Resolves all profiles from the service provider
    /// </summary>
    /// <param name="sp">The service provider</param>
    /// <returns>The resolved profiles</returns>
    private static IEnumerable<Profile> ResolveProfiles(IServiceProvider sp)
    {
        var baseInstances = sp.Resolve<IEnumerable<ProfileInstance>>().Select(x => x.Instance).ToArray();

        var typeResolver = sp.Resolve<ITypeResolver>();
        var profileTypes = sp.Resolve<IEnumerable<ProfileType>>().ToArray();

        var types = profileTypes
            .SelectMany(x => typeResolver.ResolveType(x.Type))
            .Where(x =>
                !x.IsGenericType
                || x.GetGenericArguments().All(a => a.GetCustomAttribute<AutoMappedAttribute>() is not null)
            )
            .ToArray();

        var typeInstances = types.Select(sp.Resolve).OfType<Profile>().ToArray();

        return baseInstances.Concat(typeInstances).ToArray();
    }
}
