using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium;
using Annium.Reflection;

namespace Annium.Configuration.Abstractions.Internal;

/// <summary>
/// Configuration provider that reads configuration data from .NET objects
/// </summary>
internal class ObjectConfigurationProvider : ConfigurationProviderBase
{
    private static readonly object[] _noArgs = Array.Empty<object>();

    /// <summary>
    /// The configuration object to read data from
    /// </summary>
    private readonly object? _config;

    /// <summary>
    /// Initializes a new instance of ObjectConfigurationProvider
    /// </summary>
    /// <param name="config">Configuration object to read data from</param>
    public ObjectConfigurationProvider(object? config)
    {
        _config = config;
    }

    /// <summary>
    /// Reads configuration data from the object and returns it as a dictionary
    /// </summary>
    /// <returns>Dictionary containing configuration keys and values</returns>
    public override IReadOnlyDictionary<string[], string> Read()
    {
        Init();

        if (_config is not null)
            Process(Array.Empty<string>(), _config);

        return Result;
    }

    /// <summary>
    /// Processes an object value and adds its data to the configuration
    /// </summary>
    /// <param name="prefix">Key prefix for the current path</param>
    /// <param name="value">Object value to process</param>
    private void Process(string[] prefix, object? value)
    {
        if (value is null)
            return;

        var type = value.GetType();
        var variant = Helper.GetVariant(type);
        switch (variant)
        {
            case TypeVariant.Object:
                ProcessObject(prefix, value);
                break;
            case TypeVariant.Dictionary:
                ProcessDictionary(prefix, value);
                break;
            case TypeVariant.Enumerable:
                ProcessEnumerable(prefix, value);
                break;
            case TypeVariant.Nullable:
                ProcessNullable(prefix, value);
                break;
            case TypeVariant.Primitive:
                ProcessPrimitive(prefix, value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(variant), $"Can't process type {type.FriendlyName()}");
        }
    }

    /// <summary>
    /// Processes an object by iterating through its properties and fields
    /// </summary>
    /// <param name="prefix">Key prefix for the current path</param>
    /// <param name="value">Object to process</param>
    private void ProcessObject(string[] prefix, object value)
    {
        var type = value.GetType();
        var members = Helper.GetMembers(type);

        foreach (var member in members)
        {
            var memberPath = prefix.Append(member.Name).ToArray();
            var memberValue = member.GetPropertyOrFieldValue(value);

            if (memberValue is not null)
                Process(memberPath, memberValue);
        }
    }

    /// <summary>
    /// Processes a dictionary by iterating through its key-value pairs
    /// </summary>
    /// <param name="prefix">Key prefix for the current path</param>
    /// <param name="value">Dictionary to process</param>
    private void ProcessDictionary(string[] prefix, object value)
    {
        var type = value.GetType();
        var dictionaryType =
            type.GetTargetImplementation(typeof(IDictionary<,>))
            ?? type.GetTargetImplementation(typeof(IReadOnlyDictionary<,>));
        if (dictionaryType is null)
            throw new InvalidOperationException(
                $"Value of type {type} is not assignable to IDictionary<,> or IReadOnlyDictionary<,>"
            );
        var keyType = dictionaryType.GetGenericArguments()[0];
        var valueType = dictionaryType.GetGenericArguments()[1];
        var keyValueType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
        var getKey = keyValueType.GetProperty(nameof(KeyValuePair<,>.Key)).NotNull().GetMethod.NotNull();
        var getValue = keyValueType.GetProperty(nameof(KeyValuePair<,>.Value)).NotNull().GetMethod.NotNull();

        foreach (var item in (IEnumerable)value)
        {
            var itemValue = getValue.Invoke(item, _noArgs);
            if (itemValue is null)
                continue;

            var itemKey =
                getKey.Invoke(item, _noArgs)
                ?? throw new InvalidOperationException($"Dictionary key is null at path: {string.Join('.', prefix)}");
            var keyText =
                itemKey.ToString()
                ?? throw new InvalidOperationException(
                    $"Key.ToString() returned null for type {itemKey.GetType().FullName} at path: {string.Join('.', prefix)}"
                );
            var memberPath = prefix.Append(keyText).ToArray();

            Process(memberPath, itemValue);
        }
    }

    /// <summary>
    /// Processes an enumerable by iterating through its items with indices
    /// </summary>
    /// <param name="prefix">Key prefix for the current path</param>
    /// <param name="value">Enumerable to process</param>
    private void ProcessEnumerable(string[] prefix, object value)
    {
        var index = 0;

        foreach (var item in (IEnumerable)value)
        {
            if (item is null)
                continue;

            var memberPath = prefix.Append((index++).ToString()).ToArray();

            Process(memberPath, item);
        }
    }

    /// <summary>
    /// Processes a nullable value by extracting its underlying value
    /// </summary>
    /// <param name="prefix">Key prefix for the current path</param>
    /// <param name="value">Nullable value to process</param>
    private void ProcessNullable(string[] prefix, object value)
    {
        var type = value.GetType();
        var getValueOrDefault = type.GetMethod(nameof(Nullable<>.GetValueOrDefault), Type.EmptyTypes).NotNull();
        var innerValue = getValueOrDefault.Invoke(value, _noArgs);
        if (innerValue is null)
            return;

        Process(prefix, innerValue);
    }

    /// <summary>
    /// Processes a primitive value by converting it to string
    /// </summary>
    /// <param name="prefix">Key prefix for the current path</param>
    /// <param name="value">Primitive value to process</param>
    private void ProcessPrimitive(string[] prefix, object value)
    {
        var valueString = value.ToString();
        if (valueString is null)
            return;

        SetAt(prefix, valueString);
    }
}

/// <summary>
/// Static helper class for type processing utilities
/// </summary>
file static class Helper
{
    /// <summary>
    /// Cache for type variants to improve performance. ConcurrentDictionary is required because
    /// BuildAsync loads sources in parallel via Task.WhenAll, so Read() may run concurrently.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, TypeVariant> _variants = new();

    /// <summary>
    /// Cache for type members to improve performance. ConcurrentDictionary is required because
    /// BuildAsync loads sources in parallel via Task.WhenAll, so Read() may run concurrently.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<MemberInfo>> _members = new();

    /// <summary>
    /// Gets the type variant for a given type
    /// </summary>
    /// <param name="type">Type to get variant for</param>
    /// <returns>Type variant of the specified type</returns>
    public static TypeVariant GetVariant(Type type) => _variants.GetOrAdd(type, ResolveVariant);

    /// <summary>
    /// Gets the accessible members for a given type
    /// </summary>
    /// <param name="type">Type to get members for</param>
    /// <returns>Collection of accessible members</returns>
    public static IReadOnlyCollection<MemberInfo> GetMembers(Type type) => _members.GetOrAdd(type, ResolveMembers);

    /// <summary>
    /// Resolves the type variant for a type
    /// </summary>
    /// <param name="type">Type to resolve variant for</param>
    /// <returns>Type variant of the specified type</returns>
    private static TypeVariant ResolveVariant(Type type)
    {
        if (type.IsNullableValueType())
            return TypeVariant.Nullable;

        if (
            type.GetTargetImplementation(typeof(IDictionary<,>)) is not null
            || type.GetTargetImplementation(typeof(IReadOnlyDictionary<,>)) is not null
        )
            return TypeVariant.Dictionary;

        if (type.IsEnumerable())
            return TypeVariant.Enumerable;

        if (type.IsScalar())
            return TypeVariant.Primitive;

        return TypeVariant.Object;
    }

    /// <summary>
    /// Resolves the accessible members for a type
    /// </summary>
    /// <param name="type">Type to resolve members for</param>
    /// <returns>Collection of accessible members</returns>
    private static IReadOnlyCollection<MemberInfo> ResolveMembers(Type type)
    {
        return type.GetAllProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanRead && x.GetMethod?.GetParameters().Length == 0)
            // readonly fields are skipped by ConfigurationProcessor.ProcessObject's restore (it filters
            // !IsInitOnly); excluding them here keeps the flatten and restore member sets symmetric.
            .Concat(
                type.GetAllFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(f => !f.IsInitOnly)
                    .OfType<MemberInfo>()
            )
            .ToArray();
    }
}

/// <summary>
/// Enumeration of different type variants for configuration processing
/// </summary>
file enum TypeVariant
{
    /// <summary>
    /// Regular object type
    /// </summary>
    Object,

    /// <summary>
    /// Dictionary type
    /// </summary>
    Dictionary,

    /// <summary>
    /// Enumerable type
    /// </summary>
    Enumerable,

    /// <summary>
    /// Nullable value type
    /// </summary>
    Nullable,

    /// <summary>
    /// Primitive value type
    /// </summary>
    Primitive,
}
