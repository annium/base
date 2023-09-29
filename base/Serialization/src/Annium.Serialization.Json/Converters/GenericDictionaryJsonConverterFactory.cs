using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Json.Internal.Converters;

namespace Annium.Serialization.Json.Converters;

public class GenericDictionaryJsonConverterFactory : JsonConverterFactory
{
    private static readonly ConcurrentDictionary<Type, (Type, Type)?> TypeResolutions = new();

    public override bool CanConvert(Type objectType)
    {
        return GetKeyValueType(objectType) is not null;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var (key, value) = GetKeyValueType(typeToConvert)!.Value;

        return (JsonConverter)Activator.CreateInstance(typeof(GenericDictionaryJsonConverter<,>).MakeGenericType(key, value))!;
    }

    private static (Type, Type)? GetKeyValueType(Type type) => TypeResolutions.GetOrAdd(type, ResolveKeyValueType);

    private static (Type, Type)? ResolveKeyValueType(Type type) => type
        .GetInterfaces()
        .Select<Type, (Type, Type)?>(x =>
        {
            if (x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                x.GenericTypeArguments[0].IsGenericType &&
                x.GenericTypeArguments[0].GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var args = x.GenericTypeArguments[0].GenericTypeArguments;

                return (args[0], args[1]);
            }

            return null;
        })
        .SingleOrDefault(x => x is not null);
}