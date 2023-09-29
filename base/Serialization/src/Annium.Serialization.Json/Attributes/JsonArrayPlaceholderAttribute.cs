using System;

namespace Annium.Serialization.Json.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class JsonArrayPlaceholderAttribute : Attribute
{
    /// <summary>
    /// Works in couple with <see cref="JsonAsArrayAttribute"/> to specify constant placeholder values, when serializing object as array.
    /// </summary>
    public JsonArrayPlaceholderAttribute(int order, object? value)
    {
        Order = order;
        Value = value;
    }

    /// <summary>
    /// The serialization order of the property.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// The constant value to be put in place
    /// </summary>
    public object? Value { get; }
}