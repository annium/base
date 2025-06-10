using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using X = Annium.Data.Operations.IBooleanResult<object>;

namespace Annium.Data.Operations.Serialization.Json.Internal;

/// <summary>
/// JSON converter for boolean data result types.
/// </summary>
/// <typeparam name="TD">The data type.</typeparam>
internal class BooleanDataResultConverter<TD> : ResultConverterBase<IBooleanResult<TD>>
{
    /// <summary>
    /// Reads and converts JSON to a boolean data result.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The deserialized boolean data result.</returns>
    public override IBooleanResult<TD> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var isSuccess = false;
        TD data = default!;

        var (plainErrors, labeledErrors) = ReadErrors(
            ref reader,
            options,
            (ref Utf8JsonReader r) =>
            {
                if (r.HasProperty(nameof(X.IsSuccess)))
                    isSuccess = JsonSerializer.Deserialize<bool>(ref r, options);
                else if (r.HasProperty(nameof(X.IsFailure)))
                    isSuccess = !JsonSerializer.Deserialize<bool>(ref r, options);
                else if (r.HasProperty(nameof(X.Data)))
                    data = JsonSerializer.Deserialize<TD>(ref r, options)!;
            }
        );

        var value = isSuccess ? Result.Success(data) : Result.Failure(data);

        value.Errors(plainErrors);
        value.Errors(labeledErrors);

        return value;
    }

    /// <summary>
    /// Writes a boolean data result to JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The boolean data result to serialize.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, IBooleanResult<TD> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(nameof(X.IsSuccess).CamelCase());
        JsonSerializer.Serialize(writer, value.IsSuccess, options);

        writer.WritePropertyName(nameof(X.IsFailure).CamelCase());
        JsonSerializer.Serialize(writer, value.IsFailure, options);

        writer.WritePropertyName(nameof(X.Data).CamelCase());
        JsonSerializer.Serialize(writer, value.Data, options);

        WriteErrors(writer, value, options);

        writer.WriteEndObject();
    }
}

/// <summary>
/// Factory for creating boolean data result converters.
/// </summary>
internal class BooleanDataResultConverterFactory : ResultConverterBaseFactory
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
            Activator.CreateInstance(typeof(BooleanDataResultConverter<>).MakeGenericType(typeArgs[0]))!;
    }

    /// <summary>
    /// Determines if the specified type can be converted by this factory.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type can be converted, false otherwise.</returns>
    protected override bool IsConvertibleInterface(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IBooleanResult<>);
}
