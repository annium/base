using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Json.Attributes;
using Annium.Serialization.Json.Internal.Converters;

namespace Annium.Serialization.Json.Converters;

/// <summary>
/// JSON converter factory for types decorated with JsonAsArrayAttribute, serializing objects as JSON arrays.
/// </summary>
public class ObjectArrayJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Determines whether this factory can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check for conversion support.</param>
    /// <returns>True if the type has JsonAsArrayAttribute; otherwise, false.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.GetTypeInfo().GetCustomAttribute<JsonAsArrayAttribute>() != null;
    }

    /// <summary>
    /// Creates a JSON converter for the specified type that serializes objects as arrays.
    /// </summary>
    /// <param name="typeToConvert">The type to create a converter for.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A JSON converter for the specified type.</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)
            Activator.CreateInstance(typeof(ObjectArrayJsonConverter<>).MakeGenericType(typeToConvert))!;
    }
}
