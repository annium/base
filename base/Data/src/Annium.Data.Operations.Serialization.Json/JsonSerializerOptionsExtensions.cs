using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Data.Operations.Serialization.Json.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions ConfigureForOperations(
        this JsonSerializerOptions options
    )
    {
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        AddDefaultConverters(options.Converters);

        return options;
    }

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