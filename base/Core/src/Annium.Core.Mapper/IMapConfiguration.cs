using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper;

/// <summary>
/// Represents configuration for mapping between types, containing mapping expressions, member-specific mappings, and ignored members
/// </summary>
public interface IMapConfiguration
{
    /// <summary>
    /// Gets the global mapping function that can be used to map between source and destination types
    /// </summary>
    Func<IMapContext, LambdaExpression>? MapWith { get; }

    /// <summary>
    /// Gets the member-specific mapping functions for individual properties
    /// </summary>
    IReadOnlyDictionary<PropertyInfo, Func<IMapContext, LambdaExpression>> MemberMaps { get; }

    /// <summary>
    /// Gets the collection of properties that should be ignored during mapping
    /// </summary>
    IReadOnlyCollection<PropertyInfo> IgnoredMembers { get; }
}
