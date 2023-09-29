using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper;

public interface IMapConfiguration
{
    Func<IMapContext, LambdaExpression>? MapWith { get; }
    IReadOnlyDictionary<PropertyInfo, Func<IMapContext, LambdaExpression>> MemberMaps { get; }
    IReadOnlyCollection<PropertyInfo> IgnoredMembers { get; }
}