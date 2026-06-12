using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper.Attributes;
using Annium.Core.Mapper.Internal.DependencyInjection;
using Annium.Core.Mapper.Internal.Profiles;
using Annium.Core.Mapper.Internal.Resolvers;
using Annium.Core.Runtime;
using Annium.Core.Runtime.Types;
using Annium.Logging;
using Annium.Reflection;

namespace Annium.Core.Mapper.Internal;

/// <summary>
/// Centralized registration helper for Annium.Core.Mapper. Encapsulates base services,
/// the ordered resolver chain, builtin profiles, optional autoload, and the profile
/// resolution factory in one place so the consumer-facing <see cref="ServiceContainerExtensions.AddMapper"/>
/// extension stays a thin one-shot shim.
/// </summary>
internal static class MapperRegistration
{
    /// <summary>
    /// Runs the mapper registration against the supplied container.
    /// </summary>
    /// <param name="container">The service container to populate.</param>
    /// <param name="autoload">Whether to autoload profiles via the registered type manager.</param>
    internal static void Configure(IServiceContainer container, bool autoload)
    {
        // register base services
        container.Add<IRepacker, Repacker>().Singleton();
        container.Add<IMapBuilder, MapBuilder>().Singleton();
        container.Add<IMapper, Mapper>().Singleton();
        container
            .Add(sp => new Lazy<IMapContext>(() => new MapContext(sp.Resolve<IMapper>()), true))
            .AsSelf()
            .Singleton();

        // register resolvers in priority order — first match wins in MapBuilder.ResolveMapping
        container.Add<IMapResolver, InstanceOfMapResolver>().Singleton();
        container.Add<IMapResolver, EnumerableMapResolver>().Singleton();
        container.Add<IMapResolver, ResolutionMapResolver>().Singleton();
        container.Add<IMapResolver, DictionaryConstructorMapResolver>().Singleton();
        container.Add<IMapResolver, ConstructorMapResolver>().Singleton();
        container.Add<IMapResolver, DictionaryAssignmentMapResolver>().Singleton();
        container.Add<IMapResolver, AssignmentMapResolver>().Singleton();

        // builtin profiles
        AddProfileInstance(container, new EmptyProfile());
        AddProfileInstance(container, new DefaultProfile());
        AddProfileType(container, typeof(EnumProfile<>));

        // special cases
        AddProfileInstance(container, new EnumProfile<LogLevel>());

        if (autoload)
        {
            var typeManager = container.GetTypeManager();
            foreach (var profileType in typeManager.GetImplementations(typeof(Profile)))
                AddProfileType(container, profileType);
        }

        // profile resolution factory (runs at IEnumerable<Profile> resolution time)
        container.Add(ResolveProfiles).AsSelf().Singleton();
    }

    /// <summary>
    /// Registers a profile instance as both its concrete type and a <see cref="ProfileInstance"/> marker.
    /// </summary>
    /// <typeparam name="T">The profile type.</typeparam>
    /// <param name="container">The service container.</param>
    /// <param name="profile">The profile instance.</param>
    internal static void AddProfileInstance<T>(IServiceContainer container, T profile)
        where T : Profile
    {
        container.Add(profile).AsSelf().Singleton();
        container.Add(new ProfileInstance(profile)).AsSelf().Singleton();
    }

    /// <summary>
    /// Registers a profile type as both the concrete <see cref="Type"/> and a <see cref="ProfileType"/> marker.
    /// </summary>
    /// <param name="container">The service container.</param>
    /// <param name="profileType">The profile type.</param>
    internal static void AddProfileType(IServiceContainer container, Type profileType)
    {
        container.Add(profileType).AsSelf().Singleton();
        container.Add(new ProfileType(profileType)).AsSelf().Singleton();
    }

    /// <summary>
    /// Resolves all profiles from the service provider — both pre-registered instances and discovered types.
    /// </summary>
    /// <param name="sp">The service provider.</param>
    /// <returns>The combined profile collection.</returns>
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
