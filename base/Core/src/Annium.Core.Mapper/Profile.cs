using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Annium.Core.Mapper.Internal;

namespace Annium.Core.Mapper;

public abstract class Profile
{
    internal static Profile Merge(params Profile[] profiles)
    {
        var result = new EmptyProfile();
        foreach (var (key, map) in profiles.SelectMany(c => c._mapConfigurations))
            result._mapConfigurations[key] = map;

        return result;
    }

    internal IReadOnlyDictionary<ValueTuple<Type, Type>, IMapConfiguration> MapConfigurations => _mapConfigurations;

    private readonly Dictionary<ValueTuple<Type, Type>, IMapConfiguration> _mapConfigurations =
        new();

    public void Map<TSource, TTarget>(Expression<Func<TSource, TTarget>> mapping)
    {
        var map = Map<TSource, TTarget>();

        map.With(mapping);
    }

    public void Map<TSource, TTarget>(Func<IMapContext, Expression<Func<TSource, TTarget>>> mapping)
    {
        var map = Map<TSource, TTarget>();

        map.With(mapping);
    }

    public IMapConfigurationBuilder<TSource, TTarget> Map<TSource, TTarget>()
    {
        var map = new MapConfigurationBuilder<TSource, TTarget>();

        _mapConfigurations[(typeof(TSource), typeof(TTarget))] = map.Result;

        return map;
    }
}