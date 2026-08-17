using System;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

/// <summary>
/// Attribute to mark a property as a positional command line argument
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PositionAttribute : BaseAttribute
{
    /// <summary>
    /// Gets the position of the argument in the command line
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Gets a value indicating whether the positional argument is required
    /// </summary>
    public bool IsRequired { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionAttribute"/> class.
    /// </summary>
    /// <param name="position">Zero-based index of the positional argument.</param>
    /// <param name="isRequired">Whether the argument must be supplied.</param>
    public PositionAttribute(int position, bool isRequired = true)
    {
        Position = position;
        IsRequired = isRequired;
    }
}
