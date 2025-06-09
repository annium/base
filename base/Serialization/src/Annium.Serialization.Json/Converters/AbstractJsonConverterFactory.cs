using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Json.Internal.Converters;

namespace Annium.Serialization.Json.Converters;

/// <summary>
/// JSON converter factory for abstract classes and interfaces that have known implementations.
/// </summary>
public class AbstractJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// The type manager used to resolve abstract types to concrete implementations.
    /// </summary>
    private readonly ITypeManager _typeManager;

    /// <summary>
    /// Initializes a new instance of the AbstractJsonConverterFactory class.
    /// </summary>
    /// <param name="typeManager">The type manager to use for finding implementations.</param>
    public AbstractJsonConverterFactory(ITypeManager typeManager)
    {
        _typeManager = typeManager;
    }

    /// <summary>
    /// Determines whether this factory can convert the specified type.
    /// </summary>
    /// <param name="objectType">The type to check for conversion support.</param>
    /// <returns>True if the type can be converted; otherwise, false.</returns>
    public override bool CanConvert(Type objectType)
    {
        // if object type is not interface and object type is not abstract class
        if (!objectType.IsInterface && objectType is not { IsClass: true, IsAbstract: true })
            return false;

        // if implements IEnumerable - likely will be serialized as Json Array, so not suitable for type resolution
        if (
            objectType
                .GetInterfaces()
                .Any(x =>
                    x == typeof(IEnumerable) || x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                )
        )
            return false;

        var targetType = objectType.IsGenericType ? objectType.GetGenericTypeDefinition() : objectType;

        return _typeManager.HasImplementations(targetType);
    }

    /// <summary>
    /// Creates a JSON converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to create a converter for.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A JSON converter for the specified type.</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(AbstractJsonConverter<>).MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(converterType, _typeManager)!;
    }
}
