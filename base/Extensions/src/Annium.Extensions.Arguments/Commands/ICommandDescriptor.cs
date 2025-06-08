// ReSharper disable once CheckNamespace

namespace Annium.Extensions.Arguments;

/// <summary>
/// Interface that defines command metadata including identifier and description
/// </summary>
public interface ICommandDescriptor
{
    /// <summary>
    /// Gets the unique identifier for the command
    /// </summary>
    static abstract string Id { get; }

    /// <summary>
    /// Gets the description of what the command does
    /// </summary>
    static abstract string Description { get; }
}
