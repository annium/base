using System;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

[AttributeUsage(AttributeTargets.Property)]
public class OptionAttribute : BaseAttribute
{
    public string? Alias { get; }

    public bool IsRequired { get; }

    public OptionAttribute(
        string? alias = null,
        bool isRequired = false
    )
    {
        Alias = alias;
        IsRequired = isRequired;
    }
}