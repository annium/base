using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Runtime.Types;
using Annium.Reflection;
using Annium.Serialization.Json.Converters;
using Annium.Serialization.Json.Internal.Options;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class JsonSerializerOptionsExtensions
{
    private static readonly IReadOnlyCollection<PropertyInfo> CloneableProperties = typeof(JsonSerializerOptions)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(
            x =>
                x.PropertyType.GetTargetImplementation(typeof(IEnumerable<>)) is null
                && x is { CanRead: true, CanWrite: true }
        )
        .ToArray();

    public static JsonSerializerOptions ConfigureDefault(this JsonSerializerOptions options, ITypeManager typeManager)
    {
        options.InsertConverter(0, new EnumJsonConverterFactory());
        options.InsertConverter(1, new JsonStringEnumConverter());
        options.InsertConverter(2, new MaterializableJsonConverterFactory());
        options.InsertConverter(3, new JsonNotIndentedJsonConverterFactory());
        options.InsertConverter(4, new ObjectArrayJsonConverterFactory());
        options.InsertConverter(5, new AbstractJsonConverterFactory(typeManager));
        options.InsertConverter(6, new ConstructorJsonConverterFactory());
        options.InsertConverter(7, new GenericDictionaryJsonConverterFactory());

        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.UseDefaultNamingPolicy();
        options.IncludeFields = true;

        return options;
    }

    public static JsonSerializerOptions ResetConverters(this JsonSerializerOptions options)
    {
        options.Converters.Clear();

        return options;
    }

    public static JsonSerializerOptions WithNumberHandling(
        this JsonSerializerOptions options,
        JsonNumberHandling numberHandling
    )
    {
        options.NumberHandling = numberHandling;

        return options;
    }

    public static JsonSerializerOptions InsertConverter<TConverter>(this JsonSerializerOptions options, int index)
        where TConverter : JsonConverter, new()
    {
        options.Converters.Insert(index, new TConverter());

        return options;
    }

    public static JsonSerializerOptions InsertConverter(
        this JsonSerializerOptions options,
        int index,
        JsonConverter converter
    )
    {
        options.Converters.Insert(index, converter);

        return options;
    }

    public static JsonSerializerOptions InsertConverters(
        this JsonSerializerOptions options,
        int index,
        params JsonConverter[] converters
    )
    {
        for (var i = converters.Length - 1; i >= 0; i--)
            options.Converters.Insert(index, converters[i]);

        return options;
    }

    public static JsonSerializerOptions AddConverter<TConverter>(this JsonSerializerOptions options)
        where TConverter : JsonConverter, new()
    {
        options.Converters.Add(new TConverter());

        return options;
    }

    public static JsonSerializerOptions AddConverter(this JsonSerializerOptions options, JsonConverter converter)
    {
        options.Converters.Add(converter);

        return options;
    }

    public static JsonSerializerOptions AddConverters(
        this JsonSerializerOptions options,
        params JsonConverter[] converters
    )
    {
        foreach (var converter in converters)
            options.Converters.Add(converter);

        return options;
    }

    public static JsonSerializerOptions RemoveConverter<T>(this JsonSerializerOptions opts)
        where T : JsonConverter
    {
        var factory = opts.Converters.Single(x => x.GetType() == typeof(T));
        opts.Converters.Remove(factory);

        return opts;
    }

    public static JsonSerializerOptions UseDefaultNamingPolicy(this JsonSerializerOptions options) =>
        options.UseNamingPolicy(new DefaultJsonNamingPolicy());

    public static JsonSerializerOptions UseCamelCaseNamingPolicy(this JsonSerializerOptions options) =>
        options.UseNamingPolicy(JsonNamingPolicy.CamelCase);

    public static JsonSerializerOptions Clone(this JsonSerializerOptions opts)
    {
        var clone = new JsonSerializerOptions();

        foreach (var converter in opts.Converters)
            clone.Converters.Add(converter);

        foreach (var p in CloneableProperties)
            p.SetValue(clone, p.GetValue(opts));

        return clone;
    }

    public static JsonSerializerOptions Indented(this JsonSerializerOptions opts)
    {
        opts.WriteIndented = true;

        return opts;
    }

    public static JsonSerializerOptions NotIndented(this JsonSerializerOptions opts)
    {
        opts.WriteIndented = false;

        return opts;
    }

    private static JsonSerializerOptions UseNamingPolicy(this JsonSerializerOptions options, JsonNamingPolicy policy)
    {
        options.DictionaryKeyPolicy = policy;
        options.PropertyNamingPolicy = policy;

        return options;
    }
}
