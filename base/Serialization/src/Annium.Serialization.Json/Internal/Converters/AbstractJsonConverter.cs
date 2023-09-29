using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Runtime.Types;
using Annium.Reflection;

namespace Annium.Serialization.Json.Internal.Converters;

internal class AbstractJsonConverter<T> : JsonConverter<T>
{
    private readonly ITypeManager _typeManager;

    public AbstractJsonConverter(
        ITypeManager typeManager
    )
    {
        _typeManager = typeManager;
    }

    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var doc = JsonDocument.ParseValue(ref reader);

        var type = ResolveType(doc.RootElement, typeToConvert, options).ResolveByImplementation(typeToConvert);
        if (type is null)
            throw new SerializationException($"Can't resolve concrete type for {type} by {typeToConvert}");

        return (T)JsonSerializer.Deserialize(doc.RootElement.GetRawText(), type, options)!;
    }

    public override void Write(
        Utf8JsonWriter writer,
        T value,
        JsonSerializerOptions options
    )
    {
        JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
    }

    private Type ResolveType(
        JsonElement root,
        Type baseType,
        JsonSerializerOptions options
    )
    {
        var resolutionIdProperty = _typeManager.GetResolutionIdProperty(baseType);
        if (resolutionIdProperty is not null)
            return ResolveTypeById(root, baseType, resolutionIdProperty, options);

        var resolutionKeyProperty = _typeManager.GetResolutionKeyProperty(baseType);
        if (resolutionKeyProperty is not null)
            return ResolveTypeByKey(root, baseType, resolutionKeyProperty, options);

        return ResolveTypeBySignature(root, baseType);
    }

    private Type ResolveTypeById(
        JsonElement root,
        Type baseType,
        PropertyInfo resolutionIdProperty,
        JsonSerializerOptions options
    )
    {
        var idPropertyName =
            resolutionIdProperty.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ??
            options.PropertyNamingPolicy?.ConvertName(resolutionIdProperty.Name) ??
            resolutionIdProperty.Name;
        if (!root.TryGetProperty(idPropertyName, out var idElement))
            throw new SerializationException(Error(baseType, "id property is missing"));

        var id = JsonSerializer.Deserialize<string>(idElement.GetRawText(), options)!;
        var type = _typeManager.ResolveById(id);
        if (type is null)
            throw new SerializationException(Error(baseType, $"no match for id {id}"));

        return type;
    }

    private Type ResolveTypeByKey(
        JsonElement root,
        Type baseType,
        PropertyInfo resolutionKeyProperty,
        JsonSerializerOptions options
    )
    {
        var keyPropertyName =
            resolutionKeyProperty.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ??
            options.PropertyNamingPolicy?.ConvertName(resolutionKeyProperty.Name) ??
            resolutionKeyProperty.Name;
        if (!root.TryGetProperty(keyPropertyName, out var keyElement))
            throw new SerializationException(Error(baseType, "key property is missing"));

        var key = JsonSerializer.Deserialize(keyElement.GetRawText(), resolutionKeyProperty.PropertyType, options)!;
        var type = _typeManager.ResolveByKey(key, baseType);
        if (type is null)
            throw new SerializationException(Error(baseType, $"no match for key {key}"));

        return type;
    }

    private Type ResolveTypeBySignature(
        JsonElement root,
        Type baseType
    )
    {
        var properties = root.EnumerateObject().Select(p => p.Name).ToArray();

        var type = _typeManager.ResolveBySignature(properties, baseType);
        if (type is null)
            throw new SerializationException(Error(baseType, "no matches by signature"));

        return type;
    }

    private string Error(Type baseType, string message) =>
        $"Can't resolve concrete type definition for '{baseType}': {message}";
}