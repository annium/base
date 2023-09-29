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
    // For value types and sealed classes, we can optimize and not call IsAssignableFrom.
    private static readonly bool CheckAssignableFrom = !(
        typeof(T).GetTypeInfo().IsValueType ||
        typeof(T).GetTypeInfo().IsClass &&
        typeof(T).GetTypeInfo().IsSealed
    );

    private static readonly Type NullableT = typeof(T).GetTypeInfo().IsValueType ? typeof(Nullable<>).MakeGenericType(typeof(T)) : typeof(T);

    public override bool CanConvert(Type objectType) =>
        objectType == typeof(T) || objectType == NullableT ||
        CheckAssignableFrom && typeof(T).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());

    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            Preconditions.CheckData(typeToConvert == NullableT, $"Cannot convert null value to {typeToConvert}");
            return default !;
        }

        // Handle empty strings automatically
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (value == string.Empty)
            {
                Preconditions.CheckData(typeToConvert == NullableT, $"Cannot convert null value to {typeToConvert}");
                return default !;
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

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        WriteImplementation(writer, value, options);
    }

    public abstract T ReadImplementation(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);

    public abstract void WriteImplementation(Utf8JsonWriter writer, T value, JsonSerializerOptions options);
}