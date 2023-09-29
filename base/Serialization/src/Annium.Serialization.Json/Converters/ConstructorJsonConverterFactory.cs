using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Reflection;
using Annium.Serialization.Abstractions.Attributes;
using Annium.Serialization.Json.Internal.Converters;

namespace Annium.Serialization.Json.Converters;

public class ConstructorJsonConverterFactory : JsonConverterFactory
{
    private readonly ConcurrentDictionary<Type, ConstructorJsonConverterConfiguration?> _configurations = new();

    public override bool CanConvert(Type type) => _configurations.GetOrAdd(type, GetConfiguration) is not null;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var configuration = _configurations[typeToConvert] ?? throw new ArgumentException($"Type {typeToConvert.FriendlyName()} configuration is missing");

        return (JsonConverter)Activator.CreateInstance(
            typeof(ConstructorJsonConverter<>).MakeGenericType(typeToConvert),
            configuration.Constructor,
            configuration.Parameters,
            configuration.Properties
        )!;
    }

    private ConstructorJsonConverterConfiguration? GetConfiguration(Type type)
    {
        if (!IsSuitableType(type))
            return null;

        // select non-default constructors
        var constructors = type
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(IsSuitableConstructor)
            .Select(x => (
                constructor: x,
                isSelected: x.GetCustomAttribute<DeserializationConstructorAttribute>() != null,
                paramsCount: x.GetParameters().Length
            ))
            .OrderByDescending(x => x.paramsCount)
            .ToArray();

        // if ambiguous selection - throw
        var selected = constructors.Where(x => x.isSelected).ToArray();
        if (selected.Length > 1)
            throw new JsonException($"Type {type.FriendlyName()} has multiple constructors, marked with {typeof(DeserializationConstructorAttribute).FriendlyName()}");

        if (selected.Length == 1)
            return BuildConfiguration(selected[0].constructor);

        // if many non-default constructors - won't convert
        var nonDefault = constructors.Where(x => x.paramsCount > 0).ToArray();
        if (nonDefault.Length != 1)
            return null;

        return BuildConfiguration(nonDefault[0].constructor);
    }

    private static ConstructorJsonConverterConfiguration BuildConfiguration(ConstructorInfo constructor)
    {
        var type = constructor.DeclaringType!;
        var parameters = constructor.GetParameters()
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