using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;

namespace Annium.Net.Types.Serialization.Json.Internal.Converters;

/// <summary>
/// JSON converter for Namespace types, handling serialization to/from strings.
/// </summary>
internal class NamespaceJsonConverter : JsonConverter<Namespace>
{
    /// <summary>
    /// Reads a Namespace from JSON by deserializing a string value.
    /// </summary>
    /// <param name="reader">The JSON reader</param>
    /// <param name="typeToConvert">The type being converted</param>
    /// <param name="options">Serializer options</param>
    /// <returns>The deserialized Namespace, or null if the JSON value was null</returns>
    public override Namespace? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var token = reader.GetString();

        return token?.ToNamespace();
    }

    /// <summary>
    /// Writes a Namespace to JSON as a string value.
    /// </summary>
    /// <param name="writer">The JSON writer</param>
    /// <param name="value">The Namespace value to serialize</param>
    /// <param name="options">Serializer options</param>
    public override void Write(Utf8JsonWriter writer, Namespace value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
