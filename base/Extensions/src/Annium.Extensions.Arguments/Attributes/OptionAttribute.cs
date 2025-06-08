using System;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

/// <summary>
/// Attribute to mark a property as a command line option
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class OptionAttribute : BaseAttribute
{
    /// <summary>
    /// Gets the optional alias for the option
    /// </summary>
    public string? Alias { get; }

    /// <summary>
    /// Gets a value indicating whether the option is required
    /// </summary>
    public bool IsRequired { get; }

    public OptionAttribute(string? alias = null, bool isRequired = false)
    {
        Alias = alias;
        IsRequired = isRequired;
    }
}
