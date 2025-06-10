using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Data.Operations.Serialization.Json.Internal;

namespace Annium.Data.Operations.Serialization.Json;

/// <summary>
/// Extensions for configuring JsonSerializerOptions to work with Data.Operations types
/// </summary>
public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Configures JsonSerializerOptions for serializing Data.Operations types
    /// </summary>
    /// <param name="options">The options to configure</param>
    /// <returns>The configured options</returns>
    public static JsonSerializerOptions ConfigureForOperations(this JsonSerializerOptions options)
    {
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        AddDefaultConverters(options.Converters);

        return options;
    }

    /// <summary>
    /// Adds default converters for Data.Operations types to the converter list.
    /// </summary>
    /// <param name="converters">The converter list to add to.</param>
    private static void AddDefaultConverters(IList<JsonConverter> converters)
    {
        converters.Insert(0, new BooleanDataResultConverterFactory());
        converters.Insert(0, new BooleanResultConverterFactory());
        converters.Insert(0, new StatusDataResultConverterFactory());
        converters.Insert(0, new StatusResultConverterFactory());
        converters.Insert(0, new ResultDataConverterFactory());
        converters.Insert(0, new ResultConverterFactory());
    }
}
