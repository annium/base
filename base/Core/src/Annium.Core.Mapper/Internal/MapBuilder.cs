using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;
using Annium.Core.Runtime.Types;
using Annium.Logging;
using Annium.Reflection;

namespace Annium.Core.Mapper.Internal;

internal class MapBuilder : IMapBuilder, ILogSubject
{
    public ILogger Logger { get; }
    private readonly IReadOnlyCollection<Profile> _knownProfiles;
    private readonly ITypeResolver _typeResolver;
    private readonly IEnumerable<IMapResolver> _mapResolvers;
    private readonly IRepacker _repacker;
    private readonly Lazy<IMapContext> _mapContext;
    private readonly IDictionary<ValueTuple<Type, Type>, Entry> _entries = new Dictionary<ValueTuple<Type, Type>, Entry>();
    private readonly IMapResolverContext _context;

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

    public IMapBuilder AddProfile(Action<Profile> configure)
    {
        var profile = new EmptyProfile();
        configure(profile);

        AddEntriesFromProfile(profile);

        return this;
    }

    public IMapBuilder AddProfile<T>()
        where T : Profile
        => AddProfileInternal(typeof(T));

    public IMapBuilder AddProfile(Type profileType)
    {
        if (!profileType.GetInheritanceChain().Contains(typeof(Profile)))
            throw new ArgumentException($"Type {profileType} is not inherited from {typeof(Profile)}");

        return AddProfileInternal(profileType);
    }

    public bool HasMap(Type src, Type tgt) => src == tgt || GetEntry((src, tgt)).HasMapping;

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
            this.Trace<string, string, string>("Resolved map for {src} -> {tgt} to:\n{resultView}", src.FriendlyName(), tgt.FriendlyName(), resultView);

            entry.SetMap(result.Compile());
        }

        return entry.Map;
    }

    private Mapping ResolveMapping(Type src, Type tgt)
    {
        var entry = GetEntry((src, tgt));
        lock (entry.MappingLock)
        {
            if (entry.HasMapping)
            {
                this.Trace<string, string>("Use existing mapping for {src} -> {tgt}", src.FriendlyName(), tgt.FriendlyName());
                return entry.Mapping;
            }

            entry.SetMapping(() => BuildMapping(src, tgt, entry.HasConfiguration ? entry.Configuration : MapConfiguration.Empty));
        }

        return entry.Mapping;
    }

    private Mapping BuildMapping(Type src, Type tgt, IMapConfiguration cfg)
    {
        var mapResolver = _mapResolvers.FirstOrDefault(x => x.CanResolveMap(src, tgt));
        if (mapResolver is not null)
        {
            this.Trace<string, string, string>("Build mapping for {src} -> {tgt} with {resolver}", src.FriendlyName(), tgt.FriendlyName(), mapResolver.GetType().FriendlyName());
            return mapResolver.ResolveMap(src, tgt, cfg, _context);
        }

        throw new MappingException(src, tgt, "No map found.");
    }

    private IMapBuilder AddProfileInternal(
        Type profileType
    )
    {
        var types = _typeResolver.ResolveType(profileType);

        foreach (var type in types)
        {
            var profile = _knownProfiles.SingleOrDefault(x => x.GetType() == type)
                ?? (Profile)Activator.CreateInstance(type)!;

            AddEntriesFromProfile(profile);
        }

        return this;
    }

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

    private Entry GetEntry((Type, Type) key)
    {
        lock (_entries)
        {
            if (_entries.TryGetValue(key, out var entry))
                return entry;

            this.Trace<string, string>("Create entry for {src} -> {tgt}", key.Item1.FriendlyName(), key.Item2.FriendlyName());
            return _entries[key] = Entry.Create();
        }
    }

    private class Entry
    {
        public static Entry Create() => new();

        public bool HasConfiguration => _configuration is not null;
        public IMapConfiguration Configuration => _configuration ?? throw new InvalidOperationException("Configuration is not set");
        public readonly object MappingLock = new();
        public bool HasMapping => _mapping is not null;
        public Mapping Mapping => _mapping?.Value ?? throw new InvalidOperationException("Mapping is not set");
        public readonly object MapLock = new();
        public bool HasMap => _map is not null;
        public Delegate Map => _map ?? throw new InvalidOperationException("Map is not set");
        private IMapConfiguration? _configuration;
        private Lazy<Mapping>? _mapping;
        private Delegate? _map;

        private Entry()
        {
        }

        public void SetConfiguration(IMapConfiguration configuration)
        {
            if (HasConfiguration)
                throw new InvalidOperationException("Configuration already set");

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void SetMapping(Func<Mapping> mapping)
        {
            if (HasMapping)
                throw new InvalidOperationException("Mapping already set");

            _mapping = new Lazy<Mapping>(mapping, true);
        }

        public void SetMap(Delegate map)
        {
            if (HasMap)
                throw new InvalidOperationException("Map already set");

            _map = map ?? throw new ArgumentNullException(nameof(map));
        }
    }
}