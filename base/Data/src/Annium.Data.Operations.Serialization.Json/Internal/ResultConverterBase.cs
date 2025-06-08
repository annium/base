using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Annium.Data.Operations.Serialization.Json.Internal;

/// <summary>
/// Base class for result JSON converters.
/// </summary>
/// <typeparam name="T">The result type.</typeparam>
internal abstract class ResultConverterBase<T> : JsonConverter<T>
    where T : IResultBase<T>, IResultBase
{
    /// <summary>
    /// Delegate for actions performed during JSON reading cycles.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    protected delegate void CycleAction(ref Utf8JsonReader reader);

    /// <summary>
    /// Reads error information from JSON.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="options">The serializer options.</param>
    /// <param name="runCycle">The action to run during each cycle.</param>
    /// <returns>A tuple containing plain errors and labeled errors.</returns>
    protected (IReadOnlyCollection<string>, IReadOnlyDictionary<string, IReadOnlyCollection<string>>) ReadErrors(
        ref Utf8JsonReader reader,
        JsonSerializerOptions options,
        CycleAction runCycle
    )
    {
        IReadOnlyCollection<string> plainErrors = Array.Empty<string>();
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> labeledErrors =
            new Dictionary<string, IReadOnlyCollection<string>>();

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
                labeledErrors = JsonSerializer.Deserialize<IReadOnlyDictionary<string, IReadOnlyCollection<string>>>(
                    ref reader,
                    options
                )!;
        }

        return (plainErrors, labeledErrors);
    }

    /// <summary>
    /// Writes error information to JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The result value.</param>
    /// <param name="options">The serializer options.</param>
    protected void WriteErrors(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
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

/// <summary>
/// Base factory class for result JSON converters.
/// </summary>
internal abstract class ResultConverterBaseFactory : JsonConverterFactory
{
    /// <summary>
    /// Determines whether this factory can convert the specified type.
    /// </summary>
    /// <param name="objectType">The type to check.</param>
    /// <returns>True if the type can be converted, false otherwise.</returns>
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsInterface
            ? IsConvertibleInterface(objectType)
            : objectType.GetInterfaces().Any(IsConvertibleInterface);
    }

    /// <summary>
    /// Determines if the specified interface type can be converted.
    /// </summary>
    /// <param name="type">The interface type to check.</param>
    /// <returns>True if the interface can be converted, false otherwise.</returns>
    protected abstract bool IsConvertibleInterface(Type type);

    /// <summary>
    /// Gets the interface implementation for the specified type.
    /// </summary>
    /// <param name="type">The type to get the implementation for.</param>
    /// <returns>The interface implementation type.</returns>
    protected Type GetImplementation(Type type)
    {
        if (type.IsInterface)
            return type;

        return type.GetInterfaces().First(IsConvertibleInterface);
    }
}
