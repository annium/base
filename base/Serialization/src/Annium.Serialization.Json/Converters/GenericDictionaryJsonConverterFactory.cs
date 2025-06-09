using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Json.Internal.Converters;

namespace Annium.Serialization.Json.Converters;

/// <summary>
/// JSON converter factory for generic dictionary types that implement IEnumerable&lt;KeyValuePair&lt;TKey, TValue&gt;&gt;.
/// </summary>
public class GenericDictionaryJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Cache for type resolution results to avoid repeated reflection operations.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, (Type, Type)?> _typeResolutions = new();

    /// <summary>
    /// Determines whether this factory can convert the specified type.
    /// </summary>
    /// <param name="objectType">The type to check for conversion support.</param>
    /// <returns>True if the type is a generic dictionary; otherwise, false.</returns>
    public override bool CanConvert(Type objectType)
    {
        return GetKeyValueType(objectType) is not null;
    }

    /// <summary>
    /// Creates a JSON converter for the specified dictionary type.
    /// </summary>
    /// <param name="typeToConvert">The dictionary type to create a converter for.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A JSON converter for the specified dictionary type.</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var (key, value) = GetKeyValueType(typeToConvert)!.Value;

        return (JsonConverter)
            Activator.CreateInstance(typeof(GenericDictionaryJsonConverter<,>).MakeGenericType(key, value))!;
    }

    /// <summary>
    /// Gets the key and value types for a dictionary type.
    /// </summary>
    /// <param name="type">The type to analyze.</param>
    /// <returns>A tuple containing the key and value types, or null if not a dictionary.</returns>
    private static (Type, Type)? GetKeyValueType(Type type) => _typeResolutions.GetOrAdd(type, ResolveKeyValueType);

    /// <summary>
    /// Resolves the key and value types from a dictionary type's interfaces.
    /// </summary>
    /// <param name="type">The type to resolve key-value types for.</param>
    /// <returns>A tuple containing the key and value types, or null if not a dictionary.</returns>
    private static (Type, Type)? ResolveKeyValueType(Type type) =>
        type.GetInterfaces()
            .Select<Type, (Type, Type)?>(x =>
            {
                if (
                    x.IsGenericType
                    && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    && x.GenericTypeArguments[0].IsGenericType
                    && x.GenericTypeArguments[0].GetGenericTypeDefinition() == typeof(KeyValuePair<,>)
                )
                {
                    var args = x.GenericTypeArguments[0].GenericTypeArguments;

                    return (args[0], args[1]);
                }

                return null;
            })
            .SingleOrDefault(x => x is not null);
}
