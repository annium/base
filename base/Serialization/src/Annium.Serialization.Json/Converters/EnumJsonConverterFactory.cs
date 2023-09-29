using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Abstractions.Attributes;
using Annium.Serialization.Json.Internal.Converters;

namespace Annium.Serialization.Json.Converters;

public class EnumJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var parseAttribute = typeToConvert.GetTypeInfo().GetCustomAttribute<EnumParseAttribute>();
        var configuration = Activator.CreateInstance(
            typeof(EnumJsonConverterConfiguration<>).MakeGenericType(typeToConvert),
            parseAttribute?.Separator ?? ",",
            parseAttribute?.DefaultValue
        )!;

        var flagsAttribute = typeToConvert.GetTypeInfo().GetCustomAttribute<FlagsAttribute>();
        var converterType = flagsAttribute is null ? typeof(EnumJsonConverter<>) : typeof(FlagsEnumJsonConverter<>);

        return (JsonConverter)Activator.CreateInstance(converterType.MakeGenericType(typeToConvert), configuration)!;
    }
}