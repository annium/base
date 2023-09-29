using System;
using System.Collections.Generic;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Config;

internal interface IMapperConfigInternal : IMapperConfig
{
    IReadOnlyCollection<Type> Included { get; }
    BaseTypeRef? GetBaseTypeRefFor(Type type);
    bool IsIgnored(ContextualType type);
    bool IsExcluded(ContextualType type);
    bool IsArray(ContextualType type);
    bool IsRecord(ContextualType type);
}