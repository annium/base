using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Json.Converters;

namespace Annium.Serialization.Json.Internal.Converters;

internal class ConstructorJsonConverter<T> : JsonConverter<T>
    where T : notnull
{
    private readonly ConstructorInfo _constructor;
    private readonly List<ConstructorJsonConverterConfiguration.ParameterItem> _parameters;
    private readonly IReadOnlyCollection<PropertyInfo> _properties;

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

    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
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
            var property = _properties.SingleOrDefault(x => x.Name.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));
            if (property is null)
                continue;

            properties[property] = prop.Value.Deserialize(property.PropertyType, options);
        }

        var args = parameters
            .Select((x, i) => x ?? _parameters[i].Type.DefaultValue())
            .ToArray();
        var result = _constructor.Invoke(args);
        foreach (var (property, val) in properties)
            property.SetValue(result, val);

        return (T)result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        T value,
        JsonSerializerOptions options
    )
    {
        JsonSerializer.Serialize(writer, value, options.Clone().RemoveConverter<ConstructorJsonConverterFactory>());
    }
}