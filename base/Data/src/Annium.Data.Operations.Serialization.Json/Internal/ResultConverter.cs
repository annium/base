using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using X = Annium.Data.Operations.IResult;

namespace Annium.Data.Operations.Serialization.Json.Internal;

/// <summary>
/// JSON converter for result types.
/// </summary>
internal class ResultConverter : ResultConverterBase<X>
{
    /// <summary>
    /// Reads and converts JSON to a result.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized result.</returns>
    public override X Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = Result.New();

        var (plainErrors, labeledErrors) = ReadErrors(ref reader, options, (ref _) => { });

        value.Errors(plainErrors);
        value.Errors(labeledErrors);

        return value;
    }

    /// <summary>
    /// Writes a result to JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The result to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, X value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        WriteErrors(writer, value, options);

        writer.WriteEndObject();
    }
}

/// <summary>
/// Factory for creating result converters.
/// </summary>
internal class ResultConverterFactory : ResultConverterBaseFactory
{
    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to create a converter for.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The JSON converter instance.</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new ResultConverter();
    }

    /// <summary>
    /// Determines if the specified type can be converted by this factory.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type can be converted, false otherwise.</returns>
    protected override bool IsConvertibleInterface(Type type) => type == typeof(X);
}
