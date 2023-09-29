using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Annium.Data.Operations.Serialization.Json.Internal;

internal abstract class ResultConverterBase<T> : JsonConverter<T>
    where T : IResultBase<T>, IResultBase
{
    protected delegate void CycleAction(ref Utf8JsonReader reader);

    protected (IReadOnlyCollection<string>, IReadOnlyDictionary<string, IReadOnlyCollection<string>>) ReadProperties(
        ref Utf8JsonReader reader,
        JsonSerializerOptions options,
        CycleAction runCycle
    )
    {
        IReadOnlyCollection<string> plainErrors = Array.Empty<string>();
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> labeledErrors = new Dictionary<string, IReadOnlyCollection<string>>();

        var depth = reader.CurrentDepth + 1;
        while (reader.Read())
        {
            if (reader.CurrentDepth > depth)
                continue;
            if (reader.CurrentDepth < depth)
                break;

            runCycle(ref reader);
            if (reader.HasProperty(nameof(IResultBase.PlainErrors)))
                plainErrors = JsonSerializer.Deserialize<IReadOnlyCollection<string>>(ref reader, options)!;
            else if (reader.HasProperty(nameof(IResultBase.LabeledErrors)))
                labeledErrors = JsonSerializer.Deserialize<IReadOnlyDictionary<string, IReadOnlyCollection<string>>>(ref reader, options)!;
        }

        return (plainErrors, labeledErrors);
    }

    protected void WriteErrors(
        Utf8JsonWriter writer,
        T value,
        JsonSerializerOptions options
    )
    {
        writer.WritePropertyName(nameof(IResultBase.PlainErrors).CamelCase());
        JsonSerializer.Serialize(writer, value.PlainErrors, options);

        writer.WritePropertyName(nameof(IResultBase.LabeledErrors).CamelCase());
        JsonSerializer.Serialize(
            writer,
            options.DictionaryKeyPolicy == JsonNamingPolicy.CamelCase
                ? value.LabeledErrors.ToDictionary(x => x.Key.CamelCase(), x => x.Value)
                : value.LabeledErrors,
            options
        );
    }
}

internal abstract class ResultConverterBaseFactory : JsonConverterFactory
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsInterface
            ? IsConvertibleInterface(objectType)
            : objectType.GetInterfaces().Any(IsConvertibleInterface);
    }

    protected abstract bool IsConvertibleInterface(Type type);

    protected Type GetImplementation(Type type)
    {
        if (type.IsInterface)
            return type;

        return type.GetInterfaces()
            .First(IsConvertibleInterface);
    }
}