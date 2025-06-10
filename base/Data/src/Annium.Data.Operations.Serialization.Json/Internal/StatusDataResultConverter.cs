using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using X = Annium.Data.Operations.IStatusResult<object, object>;

namespace Annium.Data.Operations.Serialization.Json.Internal;

/// <summary>
/// JSON converter for status data result types.
/// </summary>
/// <typeparam name="TS">The status type.</typeparam>
/// <typeparam name="TD">The data type.</typeparam>
internal class StatusDataResultConverter<TS, TD> : ResultConverterBase<IStatusResult<TS, TD>>
{
    /// <summary>
    /// Reads and converts JSON to a status data result.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized status data result.</returns>
    public override IStatusResult<TS, TD> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        TS status = default!;
        TD data = default!;

        var (plainErrors, labeledErrors) = ReadErrors(
            ref reader,
            options,
            (ref Utf8JsonReader r) =>
            {
                if (r.HasProperty(nameof(X.Status)))
                    status = JsonSerializer.Deserialize<TS>(ref r, options)!;
                else if (r.HasProperty(nameof(X.Data)))
                    data = JsonSerializer.Deserialize<TD>(ref r, options)!;
            }
        );

        var value = Result.Status(status, data);

        value.Errors(plainErrors);
        value.Errors(labeledErrors);

        return value;
    }

    /// <summary>
    /// Writes a status data result to JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The status data result to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, IStatusResult<TS, TD> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(nameof(X.Status).CamelCase());
        JsonSerializer.Serialize(writer, value.Status, options);

        writer.WritePropertyName(nameof(X.Data).CamelCase());
        JsonSerializer.Serialize(writer, value.Data, options);

        WriteErrors(writer, value, options);

        writer.WriteEndObject();
    }
}

/// <summary>
/// Factory for creating status data result converters.
/// </summary>
internal class StatusDataResultConverterFactory : ResultConverterBaseFactory
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

        return (JsonConverter)
            Activator.CreateInstance(typeof(StatusDataResultConverter<,>).MakeGenericType(typeArgs[0], typeArgs[1]))!;
    }

    /// <summary>
    /// Determines if the specified type can be converted by this factory.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type can be converted, false otherwise.</returns>
    protected override bool IsConvertibleInterface(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IStatusResult<,>);
}
