using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Json.Attributes;

namespace Annium.Serialization.Json.Internal.Converters;

/// <summary>
/// JSON converter that serializes objects as JSON arrays based on member order and placeholders.
/// </summary>
/// <typeparam name="T">The type to convert.</typeparam>
internal class ObjectArrayJsonConverter<T> : JsonConverter<T>
{
    /// <summary>
    /// The ordered list of members and placeholders for array serialization.
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private static readonly IReadOnlyList<object?> _members;

    /// <summary>
    /// Static constructor that initializes the member order configuration.
    /// </summary>
    static ObjectArrayJsonConverter()
    {
        var raw = typeof(T)
            .GetMembers()
            .Where(x =>
                x switch
                {
                    PropertyInfo p => p is { CanRead: true, CanWrite: true },
                    FieldInfo f => !f.IsInitOnly,
                    _ => false,
                }
            )
            .Select(x => (order: x.GetCustomAttribute<JsonPropertyOrderAttribute>()?.Order, member: x))
            .ToArray();

        _members = raw.All(x => x.order is null) ? GetAllMembersList(raw) : GetMembersWithPlaceholdersList(raw);
    }

    /// <summary>
    /// Gets all members ordered by name when no explicit order is specified.
    /// </summary>
    /// <param name="raw">The raw member collection.</param>
    /// <returns>The ordered member list.</returns>
    private static IReadOnlyList<MemberInfo> GetAllMembersList(IReadOnlyCollection<(int? order, MemberInfo member)> raw)
    {
        return raw.OrderBy(x => x.member.Name).Select(x => x.member).ToArray();
    }

    /// <summary>
    /// Gets members with placeholders ordered by explicit order attributes.
    /// </summary>
    /// <param name="raw">The raw member collection.</param>
    /// <returns>The ordered list with members and placeholders.</returns>
    private static IReadOnlyList<object?> GetMembersWithPlaceholdersList(
        IReadOnlyCollection<(int? order, MemberInfo member)> raw
    )
    {
        var result = new List<object?>();

        var items = typeof(T)
            .GetCustomAttributes<JsonArrayPlaceholderAttribute>()
            .Select(x => (x.Order as int?, x.Value))
            .Concat(raw.Select<(int? order, MemberInfo member), (int?, object?)>(x => (x.order, x.member)));

        foreach (var (order, value) in items)
        {
            if (order is null)
                continue;

            // pad with null, if needed
            while (result.Count < order.Value)
                result.Add(null!);

            // add if last
            if (result.Count == order.Value)
            {
                result.Add(value);
                continue;
            }

            // ensure null before setting
            if (result[order.Value] != null)
                throw new InvalidOperationException(
                    $"Failed to setup {typeof(ObjectArrayJsonConverter<T>).FriendlyName()}: multiple members at order {order}"
                );

            result[order.Value] = value;
        }

        return result;
    }

    /// <summary>
    /// Reads and converts JSON array to an object.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted object.</returns>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected array start");

        var depth = reader.CurrentDepth;
        var value = Activator.CreateInstance(typeof(T))!;

        foreach (var member in _members)
        {
            reader.Read();
            if (member is PropertyInfo p)
                p.SetValue(value, JsonSerializer.Deserialize(ref reader, p.PropertyType, options));
            if (member is FieldInfo f)
                f.SetValue(value, JsonSerializer.Deserialize(ref reader, f.FieldType, options));
        }

        reader.Read();

        if (reader.CurrentDepth != depth)
            throw new JsonException($"Final depth {reader.CurrentDepth} != initial depth {depth}");

        if (reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("Expected array end");

        return (T)value;
    }

    /// <summary>
    /// Writes an object as JSON array.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The object to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var member in _members)
            switch (member)
            {
                case PropertyInfo p:
                    JsonSerializer.Serialize(writer, p.GetValue(value), p.PropertyType, options);
                    break;
                case FieldInfo f:
                    JsonSerializer.Serialize(writer, f.GetValue(value), f.FieldType, options);
                    break;
                default:
                    JsonSerializer.Serialize(
                        writer,
                        member,
                        member is null ? typeof(object) : member.GetType(),
                        options
                    );
                    break;
            }

        writer.WriteEndArray();
    }
}
