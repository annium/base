using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Data.Models;
using Annium.Serialization.Json.Internal.Converters;

namespace Annium.Serialization.Json.Converters;

/// <summary>
/// JSON converter factory for types that implement IMaterializable interface.
/// </summary>
public class MaterializableJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Determines whether this factory can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check for conversion support.</param>
    /// <returns>True if the type implements IMaterializable; otherwise, false.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.GetInterfaces().Contains(typeof(IMaterializable));
    }

    /// <summary>
    /// Creates a JSON converter for the specified materializable type.
    /// </summary>
    /// <param name="typeToConvert">The type to create a converter for.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A JSON converter for the specified materializable type.</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)
            Activator.CreateInstance(typeof(MaterializableJsonConverter<>).MakeGenericType(typeToConvert))!;
    }
}
