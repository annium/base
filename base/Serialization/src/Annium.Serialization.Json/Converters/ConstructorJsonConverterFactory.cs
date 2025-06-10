using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Reflection.Types;
using Annium.Serialization.Abstractions.Attributes;
using Annium.Serialization.Json.Internal.Converters;

namespace Annium.Serialization.Json.Converters;

/// <summary>
/// JSON converter factory that creates converters for types with parameterized constructors.
/// </summary>
public class ConstructorJsonConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Cache of constructor configurations for different types.
    /// </summary>
    private readonly ConcurrentDictionary<Type, ConstructorJsonConverterConfiguration?> _configurations = new();

    /// <summary>
    /// Determines whether this factory can convert the specified type.
    /// </summary>
    /// <param name="type">The type to check for conversion support.</param>
    /// <returns>True if the type can be converted; otherwise, false.</returns>
    public override bool CanConvert(Type type) => _configurations.GetOrAdd(type, GetConfiguration) is not null;

    /// <summary>
    /// Creates a JSON converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to create a converter for.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A JSON converter for the specified type.</returns>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var configuration =
            _configurations[typeToConvert]
            ?? throw new ArgumentException($"Type {typeToConvert.FriendlyName()} configuration is missing");

        return (JsonConverter)
            Activator.CreateInstance(
                typeof(ConstructorJsonConverter<>).MakeGenericType(typeToConvert),
                configuration.Constructor,
                configuration.Parameters,
                configuration.Properties
            )!;
    }

    /// <summary>
    /// Gets the configuration for a type's constructor-based JSON conversion.
    /// </summary>
    /// <param name="type">The type to analyze.</param>
    /// <returns>The configuration if the type is suitable; otherwise, null.</returns>
    private ConstructorJsonConverterConfiguration? GetConfiguration(Type type)
    {
        if (!IsSuitableType(type))
            return null;

        // select non-default constructors
        var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(IsSuitableConstructor)
            .Select(x =>
                (
                    constructor: x,
                    isSelected: x.GetCustomAttribute<DeserializationConstructorAttribute>() != null,
                    paramsCount: x.GetParameters().Length
                )
            )
            .OrderByDescending(x => x.paramsCount)
            .ToArray();

        // if ambiguous selection - throw
        var selected = constructors.Where(x => x.isSelected).ToArray();
        if (selected.Length > 1)
            throw new JsonException(
                $"Type {type.FriendlyName()} has multiple constructors, marked with {typeof(DeserializationConstructorAttribute).FriendlyName()}"
            );

        if (selected.Length == 1)
            return BuildConfiguration(selected[0].constructor);

        // if many non-default constructors - won't convert
        var nonDefault = constructors.Where(x => x.paramsCount > 0).ToArray();
        if (nonDefault.Length != 1)
            return null;

        return BuildConfiguration(nonDefault[0].constructor);
    }

    /// <summary>
    /// Builds a configuration for the specified constructor.
    /// </summary>
    /// <param name="constructor">The constructor to build configuration for.</param>
    /// <returns>The built configuration.</returns>
    private static ConstructorJsonConverterConfiguration BuildConfiguration(ConstructorInfo constructor)
    {
        var type = constructor.DeclaringType!;
        var parameters = constructor
            .GetParameters()
            .Select(x =>
            {
                var property = type.GetProperty(x.Name!.PascalCase());
                var name = property?.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? x.Name ?? string.Empty;

                return new ConstructorJsonConverterConfiguration.ParameterItem(x.ParameterType, name);
            })
            .ToList();
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(x => x.CanWrite)
            .ToArray();

        return new ConstructorJsonConverterConfiguration(constructor, parameters, properties);
    }

    /// <summary>
    /// Determines whether the specified type is suitable for constructor-based JSON conversion.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is suitable; otherwise, false.</returns>
    private static bool IsSuitableType(Type type)
    {
        // must be class or struct
        if (type is { IsClass: false, IsValueType: false })
            return false;

        // must be not abstract and constructable
        if (type.IsAbstract || type is { IsGenericType: true, IsConstructedGenericType: false })
            return false;

        // must be object-like
        if (typeof(IEnumerable).IsAssignableFrom(type))
            return false;

        // must not be Tuple
        if (typeof(ITuple).IsAssignableFrom(type))
            return false;

        // must not be nullable struct
        if (type.IsNullableValueType())
            return false;

        return true;
    }

    /// <summary>
    /// Determines whether the specified constructor is suitable for JSON conversion.
    /// </summary>
    /// <param name="constructor">The constructor to check.</param>
    /// <returns>True if the constructor is suitable; otherwise, false.</returns>
    private static bool IsSuitableConstructor(ConstructorInfo constructor)
    {
        var type = constructor.DeclaringType;
        var constructorParams = constructor.GetParameters();

        // clone constructor is not applicable
        if (constructorParams.Length == 1 && constructorParams[0].ParameterType == type)
            return false;

        return true;
    }
}
