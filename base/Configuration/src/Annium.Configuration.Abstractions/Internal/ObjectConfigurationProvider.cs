using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Reflection;

namespace Annium.Configuration.Abstractions.Internal;

internal class ObjectConfigurationProvider : ConfigurationProviderBase
{
    private readonly object? _config;

    public ObjectConfigurationProvider(object? config)
    {
        _config = config;
    }

    public override IReadOnlyDictionary<string[], string> Read()
    {
        var result = new Dictionary<string[], string>();

        if (_config is not null)
            Process(Array.Empty<string>(), result.Add, _config);

        return result;
    }

    private void Process(string[] prefix, Action<string[], string> addValue, object? value)
    {
        if (value is null)
            return;

        var type = value.GetType();
        var variant = Helper.GetVariant(type);
        switch (variant)
        {
            case TypeVariant.Object:
                ProcessObject(prefix, addValue, value);
                break;
            case TypeVariant.Dictionary:
                ProcessDictionary(prefix, addValue, value);
                break;
            case TypeVariant.Enumerable:
                ProcessEnumerable(prefix, addValue, value);
                break;
            case TypeVariant.Nullable:
                ProcessNullable(prefix, addValue, value);
                break;
            case TypeVariant.Primitive:
                ProcessPrimitive(prefix, addValue, value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(variant), $"Can't process type {type.FriendlyName()}");
        }
    }

    private void ProcessObject(string[] prefix, Action<string[], string> addValue, object value)
    {
        var type = value.GetType();
        var members = Helper.GetMembers(type);

        foreach (var member in members)
        {
            var memberPath = prefix.Append(member.Name).ToArray();
            var memberValue = member.GetPropertyOrFieldValue(value);

            if (memberValue is not null)
                Process(memberPath, addValue, memberValue);
        }
    }

    private void ProcessDictionary(string[] prefix, Action<string[], string> addValue, object value)
    {
        var type = value.GetType();
        var dictionaryType = type.GetTargetImplementation(typeof(IDictionary<,>)) ?? type.GetTargetImplementation(typeof(IReadOnlyDictionary<,>));
        var keyType = dictionaryType!.GetGenericArguments()[0];
        var valueType = dictionaryType.GetGenericArguments()[1];
        var keyValueType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
        var getKey = keyValueType.GetProperty(nameof(KeyValuePair<object, object>.Key))!.GetMethod!;
        var getValue = keyValueType.GetProperty(nameof(KeyValuePair<object, object>.Value))!.GetMethod!;

        foreach (var item in (IEnumerable)value)
        {
            var itemValue = getValue.Invoke(item, Array.Empty<object>());
            if (itemValue is null)
                continue;

            var itemKey = getKey.Invoke(item, Array.Empty<object>())!;
            var memberPath = prefix.Append(itemKey.ToString()!).ToArray();

            Process(memberPath, addValue, itemValue);
        }
    }

    private void ProcessEnumerable(string[] prefix, Action<string[], string> addValue, object value)
    {
        var index = 0;

        foreach (var item in (IEnumerable)value)
        {
            if (item is null)
                continue;

            var memberPath = prefix.Append((index++).ToString()).ToArray();

            Process(memberPath, addValue, item);
        }
    }

    private void ProcessNullable(string[] prefix, Action<string[], string> addValue, object value)
    {
        var type = value.GetType();
        var getValueOrDefault = type.GetMethod(nameof(Nullable<bool>.GetValueOrDefault), Type.EmptyTypes)!;
        var innerValue = getValueOrDefault.Invoke(value, Array.Empty<object>());
        if (innerValue is null)
            return;

        Process(prefix, addValue, innerValue);
    }

    private void ProcessPrimitive(string[] prefix, Action<string[], string> addValue, object value)
    {
        var valueString = value.ToString();
        if (valueString is null)
            return;

        addValue(prefix, valueString);
    }
}

file static class Helper
{
    private static readonly Dictionary<Type, TypeVariant> Variants = new();
    private static readonly Dictionary<Type, IReadOnlyCollection<MemberInfo>> Members = new();

    public static TypeVariant GetVariant(Type type)
    {
        if (Variants.TryGetValue(type, out var variant))
            return variant;

        Variants[type] = variant = ResolveVariant(type);

        return variant;
    }

    public static IReadOnlyCollection<MemberInfo> GetMembers(Type type)
    {
        if (Members.TryGetValue(type, out var members))
            return members;

        Members[type] = members = ResolveMembers(type);

        return members;
    }

    private static TypeVariant ResolveVariant(Type type)
    {
        if (type.IsNullableValueType())
            return TypeVariant.Nullable;

        if (type.GetTargetImplementation(typeof(IDictionary<,>)) is not null || type.GetTargetImplementation(typeof(IReadOnlyDictionary<,>)) is not null)
            return TypeVariant.Dictionary;

        if (type.IsEnumerable())
            return TypeVariant.Enumerable;

        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type.IsEnum)
            return TypeVariant.Primitive;

        return TypeVariant.Object;
    }

    private static IReadOnlyCollection<MemberInfo> ResolveMembers(Type type)
    {
        return type.GetAllProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead && x.GetMethod?.GetParameters().Length == 0)
            .Concat(type.GetAllFields(BindingFlags.Public | BindingFlags.Instance).OfType<MemberInfo>())
            .ToArray();
    }
}

file enum TypeVariant
{
    Object,
    Dictionary,
    Enumerable,
    Nullable,
    Primitive,
}