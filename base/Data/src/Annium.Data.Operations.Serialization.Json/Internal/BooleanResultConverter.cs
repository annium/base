using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using X = Annium.Data.Operations.IBooleanResult;

namespace Annium.Data.Operations.Serialization.Json.Internal;

/// <summary>
/// JSON converter for boolean result types.
/// </summary>
internal class BooleanResultConverter : ResultConverterBase<X>
{
    /// <summary>
    /// Reads and converts JSON to a boolean result.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized boolean result.</returns>
    public override X Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var isSuccess = false;
        var (plainErrors, labeledErrors) = ReadErrors(
            ref reader,
            options,
            (ref r) =>
            {
                if (r.HasProperty(nameof(X.IsSuccess)))
                    isSuccess = JsonSerializer.Deserialize<bool>(ref r, options);
                else if (r.HasProperty(nameof(X.IsFailure)))
                    isSuccess = !JsonSerializer.Deserialize<bool>(ref r, options);
            }
        );

        var value = isSuccess ? Result.Success() : Result.Failure();
        value.Errors(plainErrors);
        value.Errors(labeledErrors);

        return value;
    }

    /// <summary>
    /// Writes a boolean result to JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The boolean result to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, X value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(nameof(X.IsSuccess).CamelCase());
        JsonSerializer.Serialize(writer, value.IsSuccess, options);

        writer.WritePropertyName(nameof(X.IsFailure).CamelCase());
        JsonSerializer.Serialize(writer, value.IsFailure, options);

        WriteErrors(writer, value, options);

        writer.WriteEndObject();
    }
}

/// <summary>
/// Factory for creating boolean result converters.
/// </summary>
internal class BooleanResultConverterFactory : ResultConverterBaseFactory
{
    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to create a converter for.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The JSON converter instance.</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new BooleanResultConverter();
    }

    /// <summary>
    /// Determines if the specified type can be converted by this factory.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type can be converted, false otherwise.</returns>
    protected override bool IsConvertibleInterface(Type type) => type == typeof(X);
}
