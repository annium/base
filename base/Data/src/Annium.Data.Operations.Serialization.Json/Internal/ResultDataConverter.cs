using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using X = Annium.Data.Operations.IResult<object>;

namespace Annium.Data.Operations.Serialization.Json.Internal;

internal class ResultDataConverter<TD> : ResultConverterBase<IResult<TD>>
{
    public override IResult<TD> Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        TD data = default!;

        var (plainErrors, labeledErrors) = ReadProperties(ref reader, options, (ref Utf8JsonReader r) =>
        {
            if (r.HasProperty(nameof(X.Data)))
                data = JsonSerializer.Deserialize<TD>(ref r, options)!;
        });

        var value = Result.New(data);

        value.Errors(plainErrors);
        value.Errors(labeledErrors);

        return value;
    }

    public override void Write(
        Utf8JsonWriter writer,
        IResult<TD> value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        writer.WritePropertyName(nameof(X.Data).CamelCase());
        JsonSerializer.Serialize(writer, value.Data, options);

        WriteErrors(writer, value, options);

        writer.WriteEndObject();
    }
}

internal class ResultDataConverterFactory : ResultConverterBaseFactory
{
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeArgs = GetImplementation(typeToConvert).GetGenericArguments();

        return (JsonConverter)Activator.CreateInstance(typeof(ResultDataConverter<>).MakeGenericType(typeArgs[0]))!;
    }

    protected override bool IsConvertibleInterface(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IResult<>);
}