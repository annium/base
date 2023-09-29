using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal;

internal class MapConfiguration : IMapConfiguration
{
    public static IMapConfiguration Empty { get; } = new MapConfiguration();
    public Func<IMapContext, LambdaExpression>? MapWith { get; private set; }
    public IReadOnlyDictionary<PropertyInfo, Func<IMapContext, LambdaExpression>> MemberMaps => _memberMaps;
    public IReadOnlyCollection<PropertyInfo> IgnoredMembers => _ignoredMembers;
    private readonly Dictionary<PropertyInfo, Func<IMapContext, LambdaExpression>> _memberMaps = new();
    private readonly HashSet<PropertyInfo> _ignoredMembers = new();

    public void SetMapWith(LambdaExpression mapWith)
    {
        MapWith = _ => mapWith;
    }

    public void SetMapWith(Func<IMapContext, LambdaExpression> mapWith)
    {
        MapWith = mapWith;
    }

    public void AddMapWithFor(IReadOnlyCollection<PropertyInfo> properties, LambdaExpression mapWith)
    {
        foreach (var property in properties)
            _memberMaps[property] = _ => mapWith;
    }

    public void AddMapWithFor(IReadOnlyCollection<PropertyInfo> properties, Func<IMapContext, LambdaExpression> mapWith)
    {
        foreach (var property in properties)
            _memberMaps[property] = mapWith;
    }

    public void Ignore(IReadOnlyCollection<PropertyInfo> properties)
    {
        foreach (var property in properties)
            _ignoredMembers.Add(property);
    }
}