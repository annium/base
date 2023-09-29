using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using X = Annium.Data.Operations.IBooleanResult;

namespace Annium.Data.Operations.Serialization.Json.Internal;

internal class BooleanResultConverter : ResultConverterBase<X>
{
    public override X Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var isSuccess = false;
        var (plainErrors, labeledErrors) = ReadProperties(ref reader, options, (ref Utf8JsonReader r) =>
        {
            if (r.HasProperty(nameof(X.IsSuccess)))
                isSuccess = JsonSerializer.Deserialize<bool>(ref r, options);
            else if (r.HasProperty(nameof(X.IsFailure)))
                isSuccess = !JsonSerializer.Deserialize<bool>(ref r, options);
        });

        var value = isSuccess ? Result.Success() : Result.Failure();
        value.Errors(plainErrors);
        value.Errors(labeledErrors);

        return value;
    }

    public override void Write(
        Utf8JsonWriter writer,
        X value,
        JsonSerializerOptions options
    )
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

internal class BooleanResultConverterFactory : ResultConverterBaseFactory
{
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new BooleanResultConverter();
    }

    protected override bool IsConvertibleInterface(Type type) => type == typeof(X);
}