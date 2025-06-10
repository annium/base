using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Annium.NodaTime.Serialization.Json.Internal.Converters;

/// <summary>
/// Base class for all the Json converters which handle value types (which is most of them).
/// This deals handles all the boilerplate code dealing with nullity.
/// </summary>
/// <typeparam name="T">The type to convert to/from JSON.</typeparam>
internal abstract class ConverterBase<T> : JsonConverter<T>
{
    /// <summary>
    /// Indicates whether assignability checking is required for the target type during conversion.
    /// </summary>
    // For value types and sealed classes, we can optimize and not call IsAssignableFrom.
    private static readonly bool _checkAssignableFrom = !(
        typeof(T).GetTypeInfo().IsValueType || typeof(T).GetTypeInfo().IsClass && typeof(T).GetTypeInfo().IsSealed
    );

    /// <summary>
    /// The nullable version of type T, used for null value handling during JSON conversion.
    /// </summary>
    private static readonly Type _nullableT = typeof(T).GetTypeInfo().IsValueType
        ? typeof(Nullable<>).MakeGenericType(typeof(T))
        : typeof(T);

    /// <summary>
    /// Determines whether this converter can convert the specified object type.
    /// </summary>
    /// <param name="objectType">The type to check for conversion support.</param>
    /// <returns>true if this converter can convert the specified type; otherwise, false.</returns>
    public override bool CanConvert(Type objectType) =>
        objectType == typeof(T)
        || objectType == _nullableT
        || _checkAssignableFrom && typeof(T).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());

    /// <summary>
    /// Reads and converts the JSON to the specified type.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The converted value.</returns>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            Preconditions.CheckData(typeToConvert == _nullableT, $"Cannot convert null value to {typeToConvert}");
            return default!;
        }

        // Handle empty strings automatically
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (value == string.Empty)
            {
                Preconditions.CheckData(typeToConvert == _nullableT, $"Cannot convert null value to {typeToConvert}");
                return default!;
            }
        }

        try
        {
            // Delegate to the concrete subclass. At this point we know that we don't want to return null, so we
            // can ask the subclass to return a T, which we will box. That will be valid even if objectType is
            // T? because the boxed form of a non-null T? value is just the boxed value itself.

            // Note that we don't currently pass existingValue down; we could change this if we ever found a use for it.
            return ReadImplementation(ref reader, typeToConvert, options);
        }
        catch (Exception ex)
        {
            throw new JsonException($"Cannot convert value to {typeToConvert}", ex);
        }
    }

    /// <summary>
    /// Writes the specified value as JSON.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        WriteImplementation(writer, value, options);
    }

    /// <summary>
    /// Reads and converts the JSON to the specified type. This method is called by the base Read method after null handling.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The converted value.</returns>
    public abstract T ReadImplementation(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);

    /// <summary>
    /// Writes the specified value as JSON. This method is called by the base Write method.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">The serializer options to use.</param>
    public abstract void WriteImplementation(Utf8JsonWriter writer, T value, JsonSerializerOptions options);
}
