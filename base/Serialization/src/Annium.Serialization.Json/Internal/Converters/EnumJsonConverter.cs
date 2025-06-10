using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Json.Converters;

namespace Annium.Serialization.Json.Internal.Converters;

/// <summary>
/// JSON converter for enum types that supports string and numeric representations.
/// </summary>
/// <typeparam name="T">The enum type to convert.</typeparam>
internal class EnumJsonConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    /// <summary>
    /// The converter configuration.
    /// </summary>
    private readonly EnumJsonConverterConfiguration<T> _cfg;

    /// <summary>
    /// Initializes a new instance of the EnumJsonConverter class.
    /// </summary>
    /// <param name="cfg">The converter configuration.</param>
    public EnumJsonConverter(EnumJsonConverterConfiguration<T> cfg)
    {
        _cfg = cfg;
    }

    /// <summary>
    /// Reads and converts JSON to an enum value.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted enum value.</returns>
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

    /// <summary>
    /// Writes an enum value as JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The enum value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options.Clone().RemoveConverter<EnumJsonConverterFactory>());
    }
}

/// <summary>
/// JSON converter for flags enum types that supports multi-value string representations.
/// </summary>
/// <typeparam name="T">The flags enum type to convert.</typeparam>
internal class FlagsEnumJsonConverter<T> : JsonConverter<T>
    where T : struct, Enum
{
    /// <summary>
    /// The converter configuration.
    /// </summary>
    private readonly EnumJsonConverterConfiguration<T> _cfg;

    /// <summary>
    /// Initializes a new instance of the FlagsEnumJsonConverter class.
    /// </summary>
    /// <param name="cfg">The converter configuration.</param>
    public FlagsEnumJsonConverter(EnumJsonConverterConfiguration<T> cfg)
    {
        _cfg = cfg;
    }

    /// <summary>
    /// Reads and converts JSON to a flags enum value.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted flags enum value.</returns>
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

    /// <summary>
    /// Writes an enum value as JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The enum value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options.Clone().RemoveConverter<EnumJsonConverterFactory>());
    }
}
