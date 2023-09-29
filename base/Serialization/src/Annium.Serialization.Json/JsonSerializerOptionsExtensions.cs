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
    public static JsonSerializerOptions ConfigureDefault(
        this JsonSerializerOptions options,
        ITypeManager typeManager
    )
    {
        options.Converters.Insert(0, new EnumJsonConverterFactory());
        options.Converters.Insert(1, new JsonStringEnumConverter());
        options.Converters.Insert(2, new MaterializableJsonConverterFactory());
        options.Converters.Insert(3, new JsonNotIndentedJsonConverterFactory());
        options.Converters.Insert(4, new ObjectArrayJsonConverterFactory());
        options.Converters.Insert(5, new AbstractJsonConverterFactory(typeManager));
        options.Converters.Insert(6, new ConstructorJsonConverterFactory());
        options.Converters.Insert(7, new GenericDictionaryJsonConverterFactory());

        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.UseDefaultNamingPolicy();
        options.IncludeFields = true;

        return options;
    }

    public static JsonSerializerOptions UseDefaultNamingPolicy(this JsonSerializerOptions options) =>
        options.UseNamingPolicy(new DefaultJsonNamingPolicy());

    public static JsonSerializerOptions UseCamelCaseNamingPolicy(this JsonSerializerOptions options) =>
        options.UseNamingPolicy(JsonNamingPolicy.CamelCase);

    private static JsonSerializerOptions UseNamingPolicy(this JsonSerializerOptions options, JsonNamingPolicy policy)
    {
        options.DictionaryKeyPolicy = policy;
        options.PropertyNamingPolicy = policy;

        return options;
    }

    private static readonly IReadOnlyCollection<PropertyInfo> CloneableProperties = typeof(JsonSerializerOptions)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(x => x.PropertyType.GetTargetImplementation(typeof(IEnumerable<>)) is null && x is { CanRead: true, CanWrite: true })
        .ToArray();

    public static JsonSerializerOptions Clone(this JsonSerializerOptions opts)
    {
        var clone = new JsonSerializerOptions();

        foreach (var converter in opts.Converters)
            clone.Converters.Add(converter);

        foreach (var p in CloneableProperties)
            p.SetValue(clone, p.GetValue(opts));

        return clone;
    }

    public static JsonSerializerOptions RemoveConverter<T>(this JsonSerializerOptions opts)
        where T : JsonConverter
    {
        var factory = opts.Converters.Single(x => x.GetType() == typeof(T));
        opts.Converters.Remove(factory);

        return opts;
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
}