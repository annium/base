using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using X = Annium.Data.Operations.IBooleanResult<object>;

namespace Annium.Data.Operations.Serialization.Json.Internal;

internal class BooleanDataResultConverter<TD> : ResultConverterBase<IBooleanResult<TD>>
{
    public override IBooleanResult<TD> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var isSuccess = false;
        TD data = default !;

        var (plainErrors, labeledErrors) = ReadProperties(ref reader, options, (ref Utf8JsonReader r) =>
        {
            if (r.HasProperty(nameof(X.IsSuccess)))
                isSuccess = JsonSerializer.Deserialize<bool>(ref r, options);
            else if (r.HasProperty(nameof(X.IsFailure)))
                isSuccess = !JsonSerializer.Deserialize<bool>(ref r, options);
            else if (r.HasProperty(nameof(X.Data)))
                data = JsonSerializer.Deserialize<TD>(ref r, options)!;
        });

        var value = isSuccess ? Result.Success(data) : Result.Failure(data);

        value.Errors(plainErrors);
        value.Errors(labeledErrors);

        return value;
    }

    public override void Write(
        Utf8JsonWriter writer,
        IBooleanResult<TD> value,
        JsonSerializerOptions options
    )
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

internal class BooleanDataResultConverterFactory : ResultConverterBaseFactory
{
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArgs = GetImplementation(typeToConvert).GetGenericArguments();

        return (JsonConverter)Activator.CreateInstance(typeof(BooleanDataResultConverter<>).MakeGenericType(typeArgs[0]))!;
    }

    protected override bool IsConvertibleInterface(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IBooleanResult<>);
}