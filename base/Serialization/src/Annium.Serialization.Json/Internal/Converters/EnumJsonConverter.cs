using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Json.Converters;

namespace Annium.Serialization.Json.Internal.Converters;

internal class EnumJsonConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    private readonly EnumJsonConverterConfiguration<T> _cfg;

    public EnumJsonConverter(EnumJsonConverterConfiguration<T> cfg)
    {
        _cfg = cfg;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return _cfg.DefaultValue.HasValue
                ? reader.GetString()!.ParseEnum(_cfg.DefaultValue.Value)
                : reader.GetString()!.ParseEnum<T>();

        if (reader.TokenType == JsonTokenType.Number)
            return _cfg.DefaultValue.HasValue
                ? reader.GetInt64().ParseEnum(_cfg.DefaultValue.Value)
                : reader.GetInt64().ParseEnum<T>();

        throw new JsonException("Enum can be parsed from string/number token only.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options.Clone().RemoveConverter<EnumJsonConverterFactory>());
    }
}

internal class FlagsEnumJsonConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    private readonly EnumJsonConverterConfiguration<T> _cfg;

    public FlagsEnumJsonConverter(EnumJsonConverterConfiguration<T> cfg)
    {
        _cfg = cfg;
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return _cfg.DefaultValue.HasValue
                ? reader.GetString()!.ParseFlags(_cfg.ValueSeparator, _cfg.DefaultValue.Value)
                : reader.GetString()!.ParseFlags<T>(_cfg.ValueSeparator);

        if (reader.TokenType == JsonTokenType.Number)
            return _cfg.DefaultValue.HasValue
                ? reader.GetInt64().ParseEnum(_cfg.DefaultValue.Value)
                : reader.GetInt64().ParseEnum<T>();

        throw new JsonException("Enum can be parsed from string/number token only.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options.Clone().RemoveConverter<EnumJsonConverterFactory>());
    }
}