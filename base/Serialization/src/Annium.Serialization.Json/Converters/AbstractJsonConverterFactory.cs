using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Json.Internal.Converters;

namespace Annium.Serialization.Json.Converters;

public class AbstractJsonConverterFactory : JsonConverterFactory
{
    private readonly ITypeManager _typeManager;

    public AbstractJsonConverterFactory(
        ITypeManager typeManager
    )
    {
        _typeManager = typeManager;
    }

    public override bool CanConvert(Type objectType)
    {
        // if object type is not interface and object type is not abstract class
        if (!objectType.IsInterface && objectType is not { IsClass: true, IsAbstract: true })
            return false;

        // if implements IEnumerable - likely will be serialized as Json Array, so not suitable for type resolution
        if (objectType.GetInterfaces().Any(
            x => x == typeof(IEnumerable) ||
                x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)
        ))
            return false;

        var targetType = objectType.IsGenericType ? objectType.GetGenericTypeDefinition() : objectType;

        return _typeManager.HasImplementations(targetType);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(AbstractJsonConverter<>).MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(converterType, _typeManager)!;
    }
}