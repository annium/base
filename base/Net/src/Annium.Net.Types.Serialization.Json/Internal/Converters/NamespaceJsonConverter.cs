using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;

namespace Annium.Net.Types.Serialization.Json.Internal.Converters;

internal class NamespaceJsonConverter : JsonConverter<Namespace>
{
    public override Namespace? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var token = reader.GetString();

        return token?.ToNamespace();
    }

    public override void Write(Utf8JsonWriter writer, Namespace value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}