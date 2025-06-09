using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Testing;

namespace Annium.NodaTime.Serialization.Json.Tests;

/// <summary>
/// Helper utilities for testing NodaTime JSON serialization converters.
/// </summary>
internal static class TestHelper
{
    /// <summary>
    /// Asserts that a value can be serialized to the expected JSON and deserialized back to the original value.
    /// </summary>
    /// <typeparam name="T">The type of value being tested.</typeparam>
    /// <param name="value">The value to serialize and deserialize.</param>
    /// <param name="expected">The expected JSON representation.</param>
    /// <param name="converters">The JSON converters to use for serialization.</param>
    internal static void AssertConversions<T>(T value, string expected, params JsonConverter[] converters)
    {
        var options = With(converters);

        var actual = JsonSerializer.Serialize(value, options);
        actual.Is(expected);

        var deserialized = JsonSerializer.Deserialize<T>(expected, options);
        deserialized.Is(value);
    }

    /// <summary>
    /// Creates JsonSerializerOptions with the specified converters and test-appropriate settings.
    /// </summary>
    /// <param name="converters">The JSON converters to include in the options.</param>
    /// <returns>Configured JsonSerializerOptions for testing.</returns>
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
