using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Runtime.Types;
using Annium.Reflection;

namespace Annium.Serialization.Json.Internal.Converters;

/// <summary>
/// JSON converter for abstract types that resolves concrete implementations using type resolution strategies.
/// </summary>
/// <typeparam name="T">The abstract type to convert.</typeparam>
internal class AbstractJsonConverter<T> : JsonConverter<T>
{
    /// <summary>
    /// The type manager used to resolve concrete implementations for abstract types.
    /// </summary>
    private readonly ITypeManager _typeManager;

    /// <summary>
    /// Initializes a new instance of the AbstractJsonConverter class.
    /// </summary>
    /// <param name="typeManager">The type manager for resolving concrete implementations.</param>
    public AbstractJsonConverter(ITypeManager typeManager)
    {
        _typeManager = typeManager;
    }

    /// <summary>
    /// Reads and converts JSON to an object of the specified type.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted object.</returns>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var doc = JsonDocument.ParseValue(ref reader);

        var type = ResolveType(doc.RootElement, typeToConvert, options).ResolveByImplementation(typeToConvert);
        if (type is null)
            throw new SerializationException($"Can't resolve concrete type for {type} by {typeToConvert}");

        return (T)JsonSerializer.Deserialize(doc.RootElement.GetRawText(), type, options)!;
    }

    /// <summary>
    /// Writes a value as JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
    }

    /// <summary>
    /// Resolves the concrete type from JSON using available resolution strategies.
    /// </summary>
    /// <param name="root">The root JSON element.</param>
    /// <param name="baseType">The base type to resolve from.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The resolved concrete type.</returns>
    private Type ResolveType(JsonElement root, Type baseType, JsonSerializerOptions options)
    {
        var resolutionIdProperty = _typeManager.GetResolutionIdProperty(baseType);
        if (resolutionIdProperty is not null)
            return ResolveTypeById(root, baseType, resolutionIdProperty, options);

        var resolutionKeyProperty = _typeManager.GetResolutionKeyProperty(baseType);
        if (resolutionKeyProperty is not null)
            return ResolveTypeByKey(root, baseType, resolutionKeyProperty, options);

        return ResolveTypeBySignature(root, baseType);
    }

    /// <summary>
    /// Resolves the concrete type using an ID property.
    /// </summary>
    /// <param name="root">The root JSON element.</param>
    /// <param name="baseType">The base type to resolve from.</param>
    /// <param name="resolutionIdProperty">The property containing the type ID.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The resolved concrete type.</returns>
    private Type ResolveTypeById(
        JsonElement root,
        Type baseType,
        PropertyInfo resolutionIdProperty,
        JsonSerializerOptions options
    )
    {
        var idPropertyName =
            resolutionIdProperty.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
            ?? options.PropertyNamingPolicy?.ConvertName(resolutionIdProperty.Name)
            ?? resolutionIdProperty.Name;
        if (!root.TryGetProperty(idPropertyName, out var idElement))
            throw new SerializationException(Error(baseType, "id property is missing"));

        var id = JsonSerializer.Deserialize<string>(idElement.GetRawText(), options)!;
        var type = _typeManager.ResolveById(id);
        if (type is null)
            throw new SerializationException(Error(baseType, $"no match for id {id}"));

        return type;
    }

    /// <summary>
    /// Resolves the concrete type using a key property.
    /// </summary>
    /// <param name="root">The root JSON element.</param>
    /// <param name="baseType">The base type to resolve from.</param>
    /// <param name="resolutionKeyProperty">The property containing the type key.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The resolved concrete type.</returns>
    private Type ResolveTypeByKey(
        JsonElement root,
        Type baseType,
        PropertyInfo resolutionKeyProperty,
        JsonSerializerOptions options
    )
    {
        var keyPropertyName =
            resolutionKeyProperty.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
            ?? options.PropertyNamingPolicy?.ConvertName(resolutionKeyProperty.Name)
            ?? resolutionKeyProperty.Name;
        if (!root.TryGetProperty(keyPropertyName, out var keyElement))
            throw new SerializationException(Error(baseType, "key property is missing"));

        var key = JsonSerializer.Deserialize(keyElement.GetRawText(), resolutionKeyProperty.PropertyType, options)!;
        var type = _typeManager.ResolveByKey(key, baseType);
        if (type is null)
            throw new SerializationException(Error(baseType, $"no match for key {key}"));

        return type;
    }

    /// <summary>
    /// Resolves the concrete type by analyzing the JSON property signature.
    /// </summary>
    /// <param name="root">The root JSON element.</param>
    /// <param name="baseType">The base type to resolve from.</param>
    /// <returns>The resolved concrete type.</returns>
    private Type ResolveTypeBySignature(JsonElement root, Type baseType)
    {
        var properties = root.EnumerateObject().Select(p => p.Name).ToArray();

        var type = _typeManager.ResolveBySignature(properties, baseType);
        if (type is null)
            throw new SerializationException(Error(baseType, "no matches by signature"));

        return type;
    }

    /// <summary>
    /// Formats an error message for type resolution failures.
    /// </summary>
    /// <param name="baseType">The base type that failed to resolve.</param>
    /// <param name="message">The specific error message.</param>
    /// <returns>The formatted error message.</returns>
    private string Error(Type baseType, string message) =>
        $"Can't resolve concrete type definition for '{baseType}': {message}";
}
