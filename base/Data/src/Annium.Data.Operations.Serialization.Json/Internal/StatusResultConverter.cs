using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using X = Annium.Data.Operations.IStatusResult<object>;

namespace Annium.Data.Operations.Serialization.Json.Internal;

internal class StatusResultConverter<TS> : ResultConverterBase<IStatusResult<TS>>
{
    public override IStatusResult<TS> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        TS status = default !;
        var (plainErrors, labeledErrors) = ReadProperties(ref reader, options, (ref Utf8JsonReader r) =>
        {
            if (r.HasProperty(nameof(X.Status)))
                status = JsonSerializer.Deserialize<TS>(ref r, options)!;
        });

        var value = Result.Status(status);

        value.Errors(plainErrors);
        value.Errors(labeledErrors);

        return value;
    }

    public override void Write(
        Utf8JsonWriter writer,
        IStatusResult<TS> value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        writer.WritePropertyName(nameof(X.Status).CamelCase());
        JsonSerializer.Serialize(writer, value.Status, options);

        WriteErrors(writer, value, options);

        writer.WriteEndObject();
    }
}

internal class StatusResultConverterFactory : ResultConverterBaseFactory
{
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArgs = GetImplementation(typeToConvert).GetGenericArguments();

        return (JsonConverter)Activator.CreateInstance(typeof(StatusResultConverter<>).MakeGenericType(typeArgs[0]))!;
    }

    protected override bool IsConvertibleInterface(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IStatusResult<>);
}