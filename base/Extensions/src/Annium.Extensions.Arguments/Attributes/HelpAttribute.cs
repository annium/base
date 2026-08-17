using System;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

/// <summary>
/// Attribute to provide help text for command line arguments
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class HelpAttribute : BaseAttribute
{
    /// <summary>
    /// Gets the help text for the argument
    /// </summary>
    public string Help { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpAttribute"/> class.
    /// </summary>
    /// <param name="help">Help text describing the annotated member.</param>
    public HelpAttribute(string help)
    {
        Help = help;
    }
}
