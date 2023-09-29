using System;
using System.Linq.Expressions;
using Annium.Reflection;

namespace Annium.Core.Mapper.Internal;

internal class MapConfigurationBuilder<TS, TD> : IMapConfigurationBuilder<TS, TD>
{
    public IMapConfiguration Result => _result;
    private readonly MapConfiguration _result = new();

    public void With(Expression<Func<TS, TD>> map)
    {
        _result.SetMapWith(map);
    }

    public void With(Func<IMapContext, Expression<Func<TS, TD>>> map)
    {
        _result.SetMapWith(map);
    }

    public IMapConfigurationBuilder<TS, TD> For<TF>(Expression<Func<TD, object>> members, Expression<Func<TS, TF>> map)
    {
        var properties = TypeHelper.ResolveProperties(members);

        _result.AddMapWithFor(properties, map);

        return this;
    }

    public IMapConfigurationBuilder<TS, TD> For<TF>(Expression<Func<TD, object>> members, Func<IMapContext, Expression<Func<TS, TF>>> map)
    {
        var properties = TypeHelper.ResolveProperties(members);

        _result.AddMapWithFor(properties, map);

        return this;
    }

    public IMapConfigurationBuilder<TS, TD> Ignore(Expression<Func<TD, object>> members)
    {
        var properties = TypeHelper.ResolveProperties(members);

        _result.Ignore(properties);

        return this;
    }
}