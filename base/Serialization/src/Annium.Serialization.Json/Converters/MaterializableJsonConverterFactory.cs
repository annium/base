using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Data.Models;
using Annium.Serialization.Json.Internal.Converters;

namespace Annium.Serialization.Json.Converters;

public class MaterializableJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.GetInterfaces().Contains(typeof(IMaterializable));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(typeof(MaterializableJsonConverter<>).MakeGenericType(typeToConvert))!;
    }
}