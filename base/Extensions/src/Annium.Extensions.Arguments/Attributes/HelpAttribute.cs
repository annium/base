using System;

namespace Annium.Extensions.Arguments.Attributes;

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

    public HelpAttribute(string help)
    {
        Help = help;
    }
}
