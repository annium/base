using System;
using System.Linq.Expressions;

namespace Annium.Core.Mapper;

/// <summary>
/// Builder interface for configuring mappings between source type TS and destination type TD
/// </summary>
/// <typeparam name="TS">Source type</typeparam>
/// <typeparam name="TD">Destination type</typeparam>
public interface IMapConfigurationBuilder<TS, TD>
{
    /// <summary>
    /// Configures a mapping expression from source to destination type
    /// </summary>
    /// <param name="map">The mapping expression</param>
    void With(Expression<Func<TS, TD>> map);

    /// <summary>
    /// Configures a contextual mapping expression from source to destination type
    /// </summary>
    /// <param name="map">The contextual mapping expression factory</param>
    void With(Func<IMapContext, Expression<Func<TS, TD>>> map);

    /// <summary>
    /// Configures mapping for specific destination members using a source expression
    /// </summary>
    /// <typeparam name="TF">Field type</typeparam>
    /// <param name="members">Expression selecting destination members</param>
    /// <param name="map">Source mapping expression</param>
    /// <returns>The configuration builder for method chaining</returns>
    IMapConfigurationBuilder<TS, TD> For<TF>(Expression<Func<TD, object>> members, Expression<Func<TS, TF>> map);

    /// <summary>
    /// Configures contextual mapping for specific destination members using a source expression factory
    /// </summary>
    /// <typeparam name="TF">Field type</typeparam>
    /// <param name="members">Expression selecting destination members</param>
    /// <param name="map">Contextual source mapping expression factory</param>
    /// <returns>The configuration builder for method chaining</returns>
    IMapConfigurationBuilder<TS, TD> For<TF>(
        Expression<Func<TD, object>> members,
        Func<IMapContext, Expression<Func<TS, TF>>> map
    );

    /// <summary>
    /// Configures destination members to be ignored during mapping
    /// </summary>
    /// <param name="members">Expression selecting destination members to ignore</param>
    /// <returns>The configuration builder for method chaining</returns>
    IMapConfigurationBuilder<TS, TD> Ignore(Expression<Func<TD, object>> members);
}
