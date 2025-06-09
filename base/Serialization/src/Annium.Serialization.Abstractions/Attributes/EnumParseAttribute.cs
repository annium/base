using System;

namespace Annium.Serialization.Abstractions.Attributes;

/// <summary>
/// Specifies how enum values should be parsed during deserialization, including custom separator and default value.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public class EnumParseAttribute : Attribute
{
    /// <summary>
    /// Gets the separator used when parsing enum values from strings.
    /// </summary>
    public string Separator { get; }

    /// <summary>
    /// Gets the default value to use when parsing fails.
    /// </summary>
    public object DefaultValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumParseAttribute"/> class.
    /// </summary>
    /// <param name="separator">The separator used when parsing enum values from strings.</param>
    /// <param name="defaultValue">The default value to use when parsing fails.</param>
    public EnumParseAttribute(string separator, object defaultValue)
    {
        Separator = separator;
        DefaultValue = defaultValue;
    }
}
