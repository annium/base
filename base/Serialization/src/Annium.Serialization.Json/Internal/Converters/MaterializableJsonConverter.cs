using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.DependencyInjection;
using Annium.Data.Models;
using Annium.Serialization.Json.Converters;

namespace Annium.Serialization.Json.Internal.Converters;

internal class MaterializableJsonConverter<T> : JsonConverter<T>
    where T : IMaterializable
{
    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = JsonSerializer.Deserialize<T>(ref reader, options.Clone().RemoveConverter<MaterializableJsonConverterFactory>())
            ?? throw new JsonException($"Failed to deserialize {typeof(T).FriendlyName()}");

        value.OnMaterialized();

        return value;
    }

    public override void Write(
        Utf8JsonWriter writer,
        T value,
        JsonSerializerOptions options
    )
    {
        JsonSerializer.Serialize(writer, value, options.Clone().RemoveConverter<MaterializableJsonConverterFactory>());
    }
}