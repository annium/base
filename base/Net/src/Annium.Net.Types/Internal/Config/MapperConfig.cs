using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Refs;
using Annium.Reflection;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Config;

internal class MapperConfig : IMapperConfigInternal
{
    #region base

    private readonly Dictionary<Type, BaseTypeRef> _baseTypes = new();

    public IMapperConfig SetBaseType(Type type, string name)
    {
        if (type is { IsClass: false, IsValueType: false })
            throw new ArgumentException($"Type {type.FriendlyName()} is neither class nor struct");

        if (type.IsGenericType || type.IsGenericTypeDefinition)
            throw new ArgumentException($"Type {type.FriendlyName()} is generic type");

        if (type.IsGenericTypeParameter)
            throw new ArgumentException($"Type {type.FriendlyName()} is generic type parameter");

        if (!_baseTypes.TryAdd(type, new BaseTypeRef(name)))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered");

        return this;
    }

    public bool IsBaseType(Type type) => _baseTypes.GetValueOrDefault(type) is not null;
    public BaseTypeRef? GetBaseTypeRefFor(Type type) => _baseTypes.GetValueOrDefault(type);

    #endregion

    #region ignored

    private readonly List<Predicate<Type>> _ignored = new();

    public IMapperConfig Ignore(Predicate<Type> matcher)
    {
        _ignored.Add(matcher);

        return this;
    }

    public bool IsIgnored(Type type)
    {
        var pure = type.GetPure();

        return _ignored.Any(match => match(pure));
    }

    public bool IsIgnored(ContextualType type) => IsIgnored(type.Type);

    #endregion

    #region include

    public IReadOnlyCollection<Type> Included => _included;
    private readonly HashSet<Type> _included = new();

    public IMapperConfig Include(Type type)
    {
        if (type != type.TryGetPure())
            throw new ArgumentException($"Can't register type {type.FriendlyName()} as included");

        if (!_included.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered as included");

        return this;
    }

    #endregion

    #region excluded

    private readonly List<Predicate<Type>> _excluded = new();

    public IMapperConfig Exclude(Predicate<Type> matcher)
    {
        _excluded.Add(matcher);

        return this;
    }

    public bool IsExcluded(Type type)
    {
        var pure = type.GetPure();

        return _excluded.Any(match => match(pure));
    }

    public bool IsExcluded(ContextualType type) => IsExcluded(type.Type);

    #endregion

    #region array

    internal static readonly Type BaseArrayType = typeof(IEnumerable<>);
    private readonly HashSet<Type> _arrayTypes = new();

    public IMapperConfig SetArray(Type type)
    {
        if (type != type.TryGetPure())
            throw new ArgumentException($"Can't register type {type.FriendlyName()} as array type");

        if (!type.IsDerivedFrom(BaseArrayType, self: true))
            throw new ArgumentException($"Type {type.FriendlyName()} doesn't implement {BaseArrayType.FriendlyName()}");

        if (!_arrayTypes.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered as array type");

        return this;
    }

    public bool IsArray(Type type)
    {
        return type.IsArray || _arrayTypes.Contains(type.GetPure());
    }

    public bool IsArray(ContextualType type) => IsArray(type.Type);

    #endregion

    #region record

    internal static readonly Type BaseRecordValueType = typeof(KeyValuePair<,>);
    private static readonly Type BaseRecordType = typeof(IEnumerable<>).MakeGenericType(BaseRecordValueType);
    private readonly HashSet<Type> _recordTypes = new();

    public IMapperConfig SetRecord(Type type)
    {
        if (type != type.TryGetPure())
            throw new ArgumentException($"Can't register type {type.FriendlyName()} as Record type");

        var arrayImplementation = type.GetInterfaces().SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == BaseArrayType);
        if (arrayImplementation is null || arrayImplementation.GetGenericArguments()[0].GetGenericTypeDefinition() != BaseRecordValueType)
            throw new ArgumentException($"Type {type.FriendlyName()} doesn't implement {BaseRecordType.FriendlyName()}");

        if (!_recordTypes.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered as Record type");

        return this;
    }

    public bool IsRecord(Type type)
    {
        return _recordTypes.Contains(type.GetPure());
    }

    public bool IsRecord(ContextualType type) => IsRecord(type.Type);

    #endregion
}