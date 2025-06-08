using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using X = Annium.Data.Operations.IResult<object>;

namespace Annium.Data.Operations.Serialization.Json.Internal;

/// <summary>
/// JSON converter for result data types.
/// </summary>
/// <typeparam name="TD">The data type.</typeparam>
internal class ResultDataConverter<TD> : ResultConverterBase<IResult<TD>>
{
    /// <summary>
    /// Reads and converts JSON to a result data.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized result data.</returns>
    public override IResult<TD> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        TD data = default!;

        var (plainErrors, labeledErrors) = ReadErrors(
            ref reader,
            options,
            (ref Utf8JsonReader r) =>
            {
                if (r.HasProperty(nameof(X.Data)))
                    data = JsonSerializer.Deserialize<TD>(ref r, options)!;
            }
        );

        var value = Result.New(data);

        value.Errors(plainErrors);
        value.Errors(labeledErrors);

        return value;
    }

    /// <summary>
    /// Writes a result data to JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The result data to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, IResult<TD> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(nameof(X.Data).CamelCase());
        JsonSerializer.Serialize(writer, value.Data, options);

        WriteErrors(writer, value, options);

        writer.WriteEndObject();
    }
}

/// <summary>
/// Factory for creating result data converters.
/// </summary>
internal class ResultDataConverterFactory : ResultConverterBaseFactory
{
    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to create a converter for.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The JSON converter instance.</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArgs = GetImplementation(typeToConvert).GetGenericArguments();

        return (JsonConverter)Activator.CreateInstance(typeof(ResultDataConverter<>).MakeGenericType(typeArgs[0]))!;
    }

    /// <summary>
    /// Determines if the specified type can be converted by this factory.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type can be converted, false otherwise.</returns>
    protected override bool IsConvertibleInterface(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IResult<>);
}
