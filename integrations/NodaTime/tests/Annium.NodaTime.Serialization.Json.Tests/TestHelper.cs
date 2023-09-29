using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Testing;

namespace Annium.NodaTime.Serialization.Json.Tests;

internal static class TestHelper
{
    internal static void AssertConversions<T>(T value, string expected, params JsonConverter[] converters)
    {
        var options = With(converters);

        var actual = JsonSerializer.Serialize(value, options);
        actual.Is(expected);

        var deserialized = JsonSerializer.Deserialize<T>(expected, options);
        deserialized.Is(value);
    }

    internal static JsonSerializerOptions With(params JsonConverter[] converters)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        foreach (var converter in converters)
            options.Converters.Add(converter);

        return options;
    }
}