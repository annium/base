using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Json.Converters;

namespace Annium.Serialization.Json.Internal.Converters;

/// <summary>
/// JSON converter that deserializes objects using constructor parameters and properties.
/// </summary>
/// <typeparam name="T">The type to convert.</typeparam>
internal class ConstructorJsonConverter<T> : JsonConverter<T>
    where T : notnull
{
    /// <summary>
    /// The constructor to use for creating instances.
    /// </summary>
    private readonly ConstructorInfo _constructor;

    /// <summary>
    /// The constructor parameters configuration.
    /// </summary>
    private readonly List<ConstructorJsonConverterConfiguration.ParameterItem> _parameters;

    /// <summary>
    /// The properties that can be set after construction.
    /// </summary>
    private readonly IReadOnlyCollection<PropertyInfo> _properties;

    /// <summary>
    /// Initializes a new instance of the ConstructorJsonConverter class.
    /// </summary>
    /// <param name="constructor">The constructor to use for creating instances.</param>
    /// <param name="parameters">The constructor parameters configuration.</param>
    /// <param name="properties">The properties that can be set after construction.</param>
    public ConstructorJsonConverter(
        ConstructorInfo constructor,
        List<ConstructorJsonConverterConfiguration.ParameterItem> parameters,
        IReadOnlyCollection<PropertyInfo> properties
    )
    {
        _constructor = constructor;
        _parameters = parameters;
        _properties = properties;
    }

    /// <summary>
    /// Reads and converts JSON to an object using constructor-based deserialization.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted object.</returns>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException();

        var parameters = new object?[_parameters.Count];
        var properties = new Dictionary<PropertyInfo, object?>();

        foreach (var prop in root.EnumerateObject())
        {
            // ignore case, when looking up for constructor parameters,
            // because it's possible, but really weird case to have parameters, differing only by case
            var index = _parameters.FindIndex(x => x.Name.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));

            // constructor parameter
            if (index >= 0)
            {
                var parameter = _parameters[index];
                var value = prop.Value.Deserialize(parameter.Type, options);
                parameters[index] = value;
                continue;
            }

            // try find property
            var property = _properties.SingleOrDefault(x =>
                x.Name.Equals(prop.Name, StringComparison.OrdinalIgnoreCase)
            );
            if (property is null)
                continue;

            properties[property] = prop.Value.Deserialize(property.PropertyType, options);
        }

        var args = parameters.Select((x, i) => x ?? _parameters[i].Type.DefaultValue()).ToArray();
        var result = _constructor.Invoke(args);
        foreach (var (property, val) in properties)
            property.SetValue(result, val);

        return (T)result;
    }

    /// <summary>
    /// Writes a value as JSON using standard serialization.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options.Clone().RemoveConverter<ConstructorJsonConverterFactory>());
    }
}
