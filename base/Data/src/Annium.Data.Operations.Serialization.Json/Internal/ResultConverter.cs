using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using X = Annium.Data.Operations.IResult;

namespace Annium.Data.Operations.Serialization.Json.Internal;

internal class ResultConverter : ResultConverterBase<X>
{
    public override X Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var value = Result.New();

        var (plainErrors, labeledErrors) = ReadProperties(ref reader, options, (ref Utf8JsonReader _) => { });

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

        WriteErrors(writer, value, options);

        writer.WriteEndObject();
    }
}

internal class ResultConverterFactory : ResultConverterBaseFactory
{
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new ResultConverter();
    }

    protected override bool IsConvertibleInterface(Type type) => type == typeof(X);
}