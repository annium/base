using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using X = Annium.Data.Operations.IStatusResult<object, object>;

namespace Annium.Data.Operations.Serialization.Json.Internal;

internal class StatusDataResultConverter<TS, TD> : ResultConverterBase<IStatusResult<TS, TD>>
{
    public override IStatusResult<TS, TD> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        TS status = default !;
        TD data = default !;

        var (plainErrors, labeledErrors) = ReadProperties(ref reader, options, (ref Utf8JsonReader r) =>
        {
            if (r.HasProperty(nameof(X.Status)))
                status = JsonSerializer.Deserialize<TS>(ref r, options)!;
            else if (r.HasProperty(nameof(X.Data)))
                data = JsonSerializer.Deserialize<TD>(ref r, options)!;
        });

        var value = Result.Status(status, data);

        value.Errors(plainErrors);
        value.Errors(labeledErrors);

        return value;
    }

    public override void Write(
        Utf8JsonWriter writer,
        IStatusResult<TS, TD> value,
        JsonSerializerOptions options
    )
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

internal class StatusDataResultConverterFactory : ResultConverterBaseFactory
{
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArgs = GetImplementation(typeToConvert).GetGenericArguments();

        return (JsonConverter)Activator.CreateInstance(typeof(StatusDataResultConverter<,>).MakeGenericType(typeArgs[0], typeArgs[1]))!;
    }

    protected override bool IsConvertibleInterface(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IStatusResult<,>);
}