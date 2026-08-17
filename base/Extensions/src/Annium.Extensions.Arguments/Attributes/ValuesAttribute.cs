using System;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

/// <summary>
/// Attribute to specify allowed values for command line arguments
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ValuesAttribute : BaseAttribute
{
    /// <summary>
    /// Gets the array of allowed values for the argument
    /// </summary>
    public string[] Values { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValuesAttribute"/> class.
    /// </summary>
    /// <param name="values">The set of values the annotated member accepts.</param>
    public ValuesAttribute(params string[] values)
    {
        Values = values;
    }
}
