using System;

namespace Annium.Serialization.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Enum)]
public class EnumParseAttribute : Attribute
{
    public string Separator { get; }
    public object DefaultValue { get; }

    public EnumParseAttribute(string separator, object defaultValue)
    {
        Separator = separator;
        DefaultValue = defaultValue;
    }
}