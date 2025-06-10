using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Abstractions.Attributes;
using Annium.Serialization.Json.Internal.Converters;

namespace Annium.Serialization.Json.Converters;

/// <summary>
/// JSON converter factory for enum types, supporting both regular and flags enums.
/// </summary>
public class EnumJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Determines whether this factory can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check for conversion support.</param>
    /// <returns>True if the type is an enum; otherwise, false.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    /// <summary>
    /// Creates a JSON converter for the specified enum type.
    /// </summary>
    /// <param name="typeToConvert">The enum type to create a converter for.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A JSON converter for the specified enum type.</returns>
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
