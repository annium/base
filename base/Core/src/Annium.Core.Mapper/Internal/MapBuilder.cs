using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;
using Annium.Core.Runtime.Types;
using Annium.Logging;
using Annium.Reflection.Types;

namespace Annium.Core.Mapper.Internal;

/// <summary>
/// Implementation of map builder that manages mapping configurations and resolves mappings between types
/// </summary>
internal class MapBuilder : IMapBuilder, ILogSubject
{
    /// <summary>
    /// Gets the logger instance
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The known profiles for mapping configurations
    /// </summary>
    private readonly IReadOnlyCollection<Profile> _knownProfiles;

    /// <summary>
    /// The type resolver for resolving generic types
    /// </summary>
    private readonly ITypeResolver _typeResolver;

    /// <summary>
    /// The collection of map resolvers
    /// </summary>
    private readonly IEnumerable<IMapResolver> _mapResolvers;

    /// <summary>
    /// The expression repacker
    /// </summary>
    private readonly IRepacker _repacker;

    /// <summary>
    /// The lazy-initialized map context
    /// </summary>
    private readonly Lazy<IMapContext> _mapContext;

    /// <summary>
    /// Cache of mapping entries by type pair
    /// </summary>
    private readonly IDictionary<ValueTuple<Type, Type>, Entry> _entries =
        new Dictionary<ValueTuple<Type, Type>, Entry>();

    /// <summary>
    /// The map resolver context
    /// </summary>
    private readonly IMapResolverContext _context;

    /// <summary>
    /// Initializes a new instance of the MapBuilder class
    /// </summary>
    /// <param name="profiles">The profiles containing mapping configurations</param>
    /// <param name="typeResolver">The type resolver for generic type resolution</param>
    /// <param name="mapResolvers">The map resolvers for different mapping scenarios</param>
    /// <param name="repacker">The expression repacker</param>
    /// <param name="mapContext">The lazy-initialized map context</param>
    /// <param name="logger">The logger instance</param>
    public MapBuilder(
        IEnumerable<Profile> profiles,
        ITypeResolver typeResolver,
        IEnumerable<IMapResolver> mapResolvers,
        IRepacker repacker,
        Lazy<IMapContext> mapContext,
        ILogger logger
    )
    {
        Logger = logger;
        _knownProfiles = profiles.ToArray();
        _typeResolver = typeResolver;
        _mapResolvers = mapResolvers;
        _repacker = repacker;
        _mapContext = mapContext;
        _context = new MapResolverContext(GetMap, ResolveMapping, mapContext);

        foreach (var profile in _knownProfiles)
            AddEntriesFromProfile(profile);
    }

    /// <summary>
    /// Adds a profile configured by the provided action
    /// </summary>
    /// <param name="configure">The profile configuration action</param>
    /// <returns>The map builder for method chaining</returns>
    public IMapBuilder AddProfile(Action<Profile> configure)
    {
        var profile = new EmptyProfile();
        configure(profile);

        AddEntriesFromProfile(profile);

        return this;
    }

    /// <summary>
    /// Adds a profile of the specified type
    /// </summary>
    /// <typeparam name="T">The profile type</typeparam>
    /// <returns>The map builder for method chaining</returns>
    public IMapBuilder AddProfile<T>()
        where T : Profile => AddProfileInternal(typeof(T));

    /// <summary>
    /// Adds a profile of the specified type
    /// </summary>
    /// <param name="profileType">The profile type</param>
    /// <returns>The map builder for method chaining</returns>
    public IMapBuilder AddProfile(Type profileType)
    {
        if (!profileType.GetInheritanceChain().Contains(typeof(Profile)))
            throw new ArgumentException($"Type {profileType} is not inherited from {typeof(Profile)}");

        return AddProfileInternal(profileType);
    }

    /// <summary>
    /// Determines if a mapping exists between the specified types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>True if a mapping exists, otherwise false</returns>
    public bool HasMap(Type src, Type tgt) => src == tgt || GetEntry((src, tgt)).HasMapping;

    /// <summary>
    /// Gets the mapping delegate between the specified types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>The mapping delegate</returns>
    public Delegate GetMap(Type src, Type tgt)
    {
        var entry = GetEntry((src, tgt));
        lock (entry.MapLock)
        {
            if (entry.HasMap)
            {
                return entry.Map;
            }

            this.Trace<string, string>("Resolve map for {src} -> {tgt}", src.FriendlyName(), tgt.FriendlyName());
            var param = Expression.Parameter(src);
            var mapping = ResolveMapping(src, tgt);

            var result = Expression.Lambda(mapping(param), param);
            var resultView = result.ToReadableString();
            this.Trace<string, string, string>(
                "Resolved map for {src} -> {tgt} to:\n{resultView}",
                src.FriendlyName(),
                tgt.FriendlyName(),
                resultView
            );

            entry.SetMap(result.Compile());
        }

        return entry.Map;
    }

    /// <summary>
    /// Resolves a mapping between the specified types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>The resolved mapping</returns>
    private Mapping ResolveMapping(Type src, Type tgt)
    {
        var entry = GetEntry((src, tgt));
        lock (entry.MappingLock)
        {
            if (entry.HasMapping)
            {
                this.Trace<string, string>(
                    "Use existing mapping for {src} -> {tgt}",
                    src.FriendlyName(),
                    tgt.FriendlyName()
                );
                return entry.Mapping;
            }

            entry.SetMapping(() =>
                BuildMapping(src, tgt, entry.HasConfiguration ? entry.Configuration : MapConfiguration.Empty)
            );
        }

        return entry.Mapping;
    }

    /// <summary>
    /// Builds a mapping between the specified types using the provided configuration
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <param name="cfg">The mapping configuration</param>
    /// <returns>The built mapping</returns>
    private Mapping BuildMapping(Type src, Type tgt, IMapConfiguration cfg)
    {
        var mapResolver = _mapResolvers.FirstOrDefault(x => x.CanResolveMap(src, tgt));
        if (mapResolver is not null)
        {
            this.Trace<string, string, string>(
                "Build mapping for {src} -> {tgt} with {resolver}",
                src.FriendlyName(),
                tgt.FriendlyName(),
                mapResolver.GetType().FriendlyName()
            );
            return mapResolver.ResolveMap(src, tgt, cfg, _context);
        }

        throw new MappingException(src, tgt, "No map found.");
    }

    /// <summary>
    /// Adds a profile type and resolves its generic variants
    /// </summary>
    /// <param name="profileType">The profile type to add</param>
    /// <returns>The map builder for method chaining</returns>
    private IMapBuilder AddProfileInternal(Type profileType)
    {
        var types = _typeResolver.ResolveType(profileType);

        foreach (var type in types)
        {
            var profile =
                _knownProfiles.SingleOrDefault(x => x.GetType() == type) ?? (Profile)Activator.CreateInstance(type)!;

            AddEntriesFromProfile(profile);
        }

        return this;
    }

    /// <summary>
    /// Adds mapping entries from the specified profile
    /// </summary>
    /// <param name="profile">The profile to add entries from</param>
    private void AddEntriesFromProfile(Profile profile)
    {
        foreach (var (key, cfg) in profile.MapConfigurations)
        {
            var entry = GetEntry(key);
            if (!entry.HasConfiguration)
                entry.SetConfiguration(cfg);
            if (!entry.HasMapping && cfg.MapWith is not null)
                entry.SetMapping(() => _repacker.Repack(cfg.MapWith(_mapContext.Value).Body));
        }
    }

    /// <summary>
    /// Gets or creates a mapping entry for the specified type pair
    /// </summary>
    /// <param name="key">The type pair key</param>
    /// <returns>The mapping entry</returns>
    private Entry GetEntry((Type, Type) key)
    {
        lock (_entries)
        {
            if (_entries.TryGetValue(key, out var entry))
                return entry;

            this.Trace<string, string>(
                "Create entry for {src} -> {tgt}",
                key.Item1.FriendlyName(),
                key.Item2.FriendlyName()
            );
            return _entries[key] = Entry.Create();
        }
    }

    /// <summary>
    /// Represents a mapping entry that stores configuration, mapping, and compiled map
    /// </summary>
    private class Entry
    {
        /// <summary>
        /// Creates a new entry instance
        /// </summary>
        /// <returns>A new entry instance</returns>
        public static Entry Create() => new();

        /// <summary>
        /// Gets a value indicating whether the entry has a configuration
        /// </summary>
        public bool HasConfiguration => _configuration is not null;

        /// <summary>
        /// Gets the mapping configuration
        /// </summary>
        public IMapConfiguration Configuration =>
            _configuration ?? throw new InvalidOperationException("Configuration is not set");

        /// <summary>
        /// Lock object for mapping operations
        /// </summary>
        public readonly object MappingLock = new();

        /// <summary>
        /// Gets a value indicating whether the entry has a mapping
        /// </summary>
        public bool HasMapping => _mapping is not null;

        /// <summary>
        /// Gets the mapping function
        /// </summary>
        public Mapping Mapping => _mapping?.Value ?? throw new InvalidOperationException("Mapping is not set");

        /// <summary>
        /// Lock object for map operations
        /// </summary>
        public readonly object MapLock = new();

        /// <summary>
        /// Gets a value indicating whether the entry has a compiled map
        /// </summary>
        public bool HasMap => _map is not null;

        /// <summary>
        /// Gets the compiled mapping delegate
        /// </summary>
        public Delegate Map => _map ?? throw new InvalidOperationException("Map is not set");

        /// <summary>
        /// The mapping configuration
        /// </summary>
        private IMapConfiguration? _configuration;

        /// <summary>
        /// The lazy-initialized mapping function
        /// </summary>
        private Lazy<Mapping>? _mapping;

        /// <summary>
        /// The compiled mapping delegate
        /// </summary>
        private Delegate? _map;

        private Entry() { }

        /// <summary>
        /// Sets the mapping configuration
        /// </summary>
        /// <param name="configuration">The configuration to set</param>
        public void SetConfiguration(IMapConfiguration configuration)
        {
            if (HasConfiguration)
                throw new InvalidOperationException("Configuration already set");

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Sets the mapping function factory
        /// </summary>
        /// <param name="mapping">The mapping factory function</param>
        public void SetMapping(Func<Mapping> mapping)
        {
            if (HasMapping)
                throw new InvalidOperationException("Mapping already set");

            _mapping = new Lazy<Mapping>(mapping, true);
        }

        /// <summary>
        /// Sets the compiled mapping delegate
        /// </summary>
        /// <param name="map">The compiled mapping delegate</param>
        public void SetMap(Delegate map)
        {
            if (HasMap)
                throw new InvalidOperationException("Map already set");

            _map = map ?? throw new ArgumentNullException(nameof(map));
        }
    }
}
