using System;
using System.Linq.Expressions;
using Annium.Reflection;

namespace Annium.Core.Mapper.Internal;

/// <summary>
/// Implementation of map configuration builder for building mapping configurations between types
/// </summary>
/// <typeparam name="TS">Source type</typeparam>
/// <typeparam name="TD">Destination type</typeparam>
internal class MapConfigurationBuilder<TS, TD> : IMapConfigurationBuilder<TS, TD>
{
    /// <summary>
    /// Gets the resulting map configuration
    /// </summary>
    internal IMapConfiguration Result => _result;

    /// <summary>
    /// The configuration being built
    /// </summary>
    private readonly MapConfiguration _result = new();

    /// <summary>
    /// Configures a mapping expression from source to destination type.
    /// </summary>
    /// <param name="map">The mapping expression.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public IMapConfigurationBuilder<TS, TD> With(Expression<Func<TS, TD>> map)
    {
        if (_result.MapWith is not null)
            throw new InvalidOperationException(
                $"With() for Map<{typeof(TS).FriendlyName()}, {typeof(TD).FriendlyName()}> is already configured."
            );

        _result.SetMapWith(map);
        return this;
    }

    /// <summary>
    /// Configures a contextual mapping expression from source to destination type.
    /// </summary>
    /// <param name="map">The contextual mapping expression factory.</param>
    /// <returns>The configuration builder for method chaining.</returns>
    public IMapConfigurationBuilder<TS, TD> With(Func<IMapContext, Expression<Func<TS, TD>>> map)
    {
        if (_result.MapWith is not null)
            throw new InvalidOperationException(
                $"With() for Map<{typeof(TS).FriendlyName()}, {typeof(TD).FriendlyName()}> is already configured."
            );

        _result.SetMapWith(map);
        return this;
    }

    /// <summary>
    /// Configures mapping for specific destination members using a source expression
    /// </summary>
    /// <typeparam name="TF">Field type</typeparam>
    /// <param name="members">Expression selecting destination members</param>
    /// <param name="map">Source mapping expression</param>
    /// <returns>The configuration builder for method chaining</returns>
    public IMapConfigurationBuilder<TS, TD> For<TF>(Expression<Func<TD, object>> members, Expression<Func<TS, TF>> map)
    {
        var properties = TypeHelper.ResolveProperties(members);

        _result.AddMapWithFor(properties, map);

        return this;
    }

    /// <summary>
    /// Configures contextual mapping for specific destination members using a source expression factory
    /// </summary>
    /// <typeparam name="TF">Field type</typeparam>
    /// <param name="members">Expression selecting destination members</param>
    /// <param name="map">Contextual source mapping expression factory</param>
    /// <returns>The configuration builder for method chaining</returns>
    public IMapConfigurationBuilder<TS, TD> For<TF>(
        Expression<Func<TD, object>> members,
        Func<IMapContext, Expression<Func<TS, TF>>> map
    )
    {
        var properties = TypeHelper.ResolveProperties(members);

        _result.AddMapWithFor(properties, map);

        return this;
    }

    /// <summary>
    /// Configures destination members to be ignored during mapping
    /// </summary>
    /// <param name="members">Expression selecting destination members to ignore</param>
    /// <returns>The configuration builder for method chaining</returns>
    public IMapConfigurationBuilder<TS, TD> Ignore(Expression<Func<TD, object>> members)
    {
        var properties = TypeHelper.ResolveProperties(members);

        _result.Ignore(properties);

        return this;
    }
}
