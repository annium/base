using System;
using System.Text.Json;
using NodaTime.Text;
using NodaTime.Utility;

namespace Annium.NodaTime.Serialization.Json.Internal.Converters;

/// <summary>
/// A JSON converter for types which can be represented by a single string value, parsed or formatted
/// from an <see cref="IPattern{T}"/>.
/// </summary>
/// <typeparam name="T">The type to convert to/from JSON.</typeparam>
internal sealed class NodaPatternConverter<T> : ConverterBase<T>
{
    /// <summary>
    /// The pattern used for parsing and formatting values of type T.
    /// </summary>
    private readonly IPattern<T> _pattern;

    /// <summary>
    /// The validator called before writing values to ensure they can be serialized.
    /// </summary>
    private readonly Action<T> _validator;

    /// <summary>
    /// Creates a new instance with a pattern and no validator.
    /// </summary>
    /// <param name="pattern">The pattern to use for parsing and formatting.</param>
    /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
    public NodaPatternConverter(IPattern<T> pattern)
        : this(pattern, _ => { }) { }

    /// <summary>
    /// Creates a new instance with a pattern and an optional validator. The validator will be called before each
    /// value is written, and may throw an exception to indicate that the value cannot be serialized.
    /// </summary>
    /// <param name="pattern">The pattern to use for parsing and formatting.</param>
    /// <param name="validator">The validator to call before writing values. May be null, indicating that no validation is required.</param>
    /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null.</exception>
    public NodaPatternConverter(IPattern<T> pattern, Action<T> validator)
    {
        Preconditions.CheckNotNull(pattern, nameof(pattern));
        _pattern = pattern;
        _validator = validator;
    }

    /// <summary>
    /// Reads a JSON string and parses it using the configured pattern.
    /// </summary>
    /// <param name="reader">The JSON reader to read from.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The parsed value.</returns>
    public override T ReadImplementation(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new InvalidNodaDataException(
                $"Unexpected token parsing {typeof(T).Name}. Expected String, got {reader.TokenType}."
            );

        return _pattern.Parse(reader.GetString()!).Value;
    }

    /// <summary>
    /// Writes a value as a JSON string using the configured pattern after validation.
    /// </summary>
    /// <param name="writer">The JSON writer to write to.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void WriteImplementation(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        _validator.Invoke(value);
        writer.WriteStringValue(_pattern.Format(value));
    }
}
