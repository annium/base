using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;
using Annium.Logging;
using Annium.Reflection;

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
    private readonly Dictionary<ValueTuple<Type, Type>, Entry> _entries = new();

    /// <summary>
    /// The map resolver context
    /// </summary>
    private readonly IMapResolverContext _context;

    /// <summary>
    /// Initializes a new instance of the MapBuilder class
    /// </summary>
    /// <param name="profiles">The profiles containing mapping configurations</param>
    /// <param name="mapResolvers">The map resolvers for different mapping scenarios</param>
    /// <param name="repacker">The expression repacker</param>
    /// <param name="mapContext">The lazy-initialized map context</param>
    /// <param name="logger">The logger instance</param>
    public MapBuilder(
        IEnumerable<Profile> profiles,
        IEnumerable<IMapResolver> mapResolvers,
        IRepacker repacker,
        Lazy<IMapContext> mapContext,
        ILogger logger
    )
    {
        Logger = logger;
        _knownProfiles = profiles.ToArray();
        _mapResolvers = mapResolvers;
        _repacker = repacker;
        _mapContext = mapContext;
        _context = new MapResolverContext(GetMap, ResolveMapping, mapContext);

        foreach (var profile in _knownProfiles)
            AddEntriesFromProfile(profile);
    }

    /// <summary>
    /// Determines if a mapping exists between the specified types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>True if a mapping exists, otherwise false</returns>
    public bool HasMap(Type src, Type tgt)
    {
        if (src == tgt)
            return true;

        var entry = GetEntry((src, tgt));
        // HasMap answers "is there an explicitly configured mapping for this pair?", NOT the broader
        // "would any resolver produce one?". We therefore probe HasConfiguration (set ONLY by
        // AddEntriesFromProfile) instead of HasMapping (which also becomes true after any GetMap()
        // call materialises a resolver-built mapping). Downstream consumers (e.g. ConfigurationProcessor)
        // use HasMap as a discriminator between atomic-value leafs (configured map exists) and complex
        // objects (recurse); broadening the probe to the resolver chain — or to side effects of prior
        // GetMap calls — would misclassify any type with a default constructor as a leaf via
        // AssignmentMapResolver and break that consumer.
        return entry.HasConfiguration;
    }

    /// <summary>
    /// Gets the mapping delegate between the specified types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>The mapping delegate</returns>
    public Delegate GetMap(Type src, Type tgt)
    {
        var entry = GetEntry((src, tgt));

        // Lock-free fast path for the steady-state case where the compiled delegate already exists.
        // The _map field is a Delegate? reference; its transition is one-way (null → non-null) and
        // reference reads are atomic on .NET, so observing a non-null Map is sufficient — we will
        // never read a torn or post-cleared value here.
        if (entry.HasMap)
            return entry.Map;

        // Resolve the mapping BEFORE acquiring MapLock. Holding MapLock across ResolveMapping (which
        // takes MappingLock and can recursively call back into GetMap for sibling type pairs) creates
        // an AB/BA cycle under concurrent compilation of circularly-related type graphs.
        var mapping = ResolveMapping(src, tgt);

        lock (entry.MapLock)
        {
            if (entry.HasMap)
                return entry.Map;

            this.Trace<string, string>("Resolve map for {src} -> {tgt}", src.FriendlyName(), tgt.FriendlyName());
            var param = Expression.Parameter(src);
            var result = Expression.Lambda(mapping(param), param);
            var resultView = result.ToReadableString();
            this.Trace<string, string, string>(
                "Resolved map for {src} -> {tgt} to:\n{resultView}",
                src.FriendlyName(),
                tgt.FriendlyName(),
                resultView
            );

            var compiled = result.Compile();
            entry.SetMap(compiled);
            // return the local delegate (rather than re-reading entry.Map outside the lock) so the
            // visibility of the just-published _map field cannot regress across memory models.
            return compiled;
        }
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
    /// Adds mapping entries from the specified profile
    /// </summary>
    /// <param name="profile">The profile to add entries from</param>
    private void AddEntriesFromProfile(Profile profile)
    {
        foreach (var (key, cfg) in profile.MapConfigurations)
        {
            var entry = GetEntry(key);
            // serialize configuration / mapping writes under MappingLock; treat duplicate (src, tgt)
            // registrations across profiles as "first profile wins" — skip BOTH the configuration
            // and the mapping for the second profile. The prior code skipped config but still
            // applied the second profile's MapWith, producing a split-brain mapping whose
            // expression and configuration came from different profiles.
            lock (entry.MappingLock)
            {
                if (entry.HasConfiguration)
                {
                    this.Trace<string, string>(
                        "Skip duplicate mapping registration for {src} -> {tgt} (first profile wins)",
                        key.Item1.FriendlyName(),
                        key.Item2.FriendlyName()
                    );
                    continue;
                }

                entry.SetConfiguration(cfg);
                if (cfg.MapWith is not null)
                    entry.SetMapping(() => _repacker.Repack(cfg.MapWith(_mapContext.Value).Body));
            }
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
        /// The lazy-initialized mapping function. Marked <c>volatile</c> so the lock-free fast-path
        /// reads in <see cref="HasMapping"/> / <see cref="Mapping"/> acquire the publication fence
        /// paired with the release fence performed by <see cref="SetMapping"/> under <see cref="MappingLock"/>.
        /// </summary>
        private volatile Lazy<Mapping>? _mapping;

        /// <summary>
        /// The compiled mapping delegate. Marked <c>volatile</c> so the lock-free fast-path reads in
        /// <see cref="HasMap"/> / <see cref="Map"/> acquire the publication fence paired with the
        /// release fence performed by <see cref="SetMap"/> under <see cref="MapLock"/>.
        /// </summary>
        private volatile Delegate? _map;

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
