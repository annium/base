using System.Collections.Generic;

namespace Annium.Extensions.Arguments.Internal;

/// <summary>
/// Contains the raw parsed data from command line arguments organized by argument type.
/// Represents the structured output of the argument processing phase before type conversion.
/// </summary>
internal class RawConfiguration
{
    /// <summary>
    /// Gets the collection of positional arguments in the order they appeared on the command line.
    /// </summary>
    public IReadOnlyCollection<string> Positions { get; }

    /// <summary>
    /// Gets the collection of flag names that were present on the command line.
    /// </summary>
    public IReadOnlyCollection<string> Flags { get; }

    /// <summary>
    /// Gets the dictionary of single-value options mapping option names to their values.
    /// </summary>
    public IReadOnlyDictionary<string, string> Options { get; }

    /// <summary>
    /// Gets the dictionary of multi-value options mapping option names to their value collections.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> MultiOptions { get; }

    /// <summary>
    /// Gets the raw argument string that appeared after the delimiter on the command line.
    /// </summary>
    public string Raw { get; }

    /// <summary>
    /// Initializes a new instance of the RawConfiguration class with parsed argument data.
    /// </summary>
    /// <param name="positions">Collection of positional arguments</param>
    /// <param name="flags">Collection of flag names that were present</param>
    /// <param name="options">Dictionary of single-value options</param>
    /// <param name="multiOptions">Dictionary of multi-value options</param>
    /// <param name="raw">Raw argument string after delimiter</param>
    public RawConfiguration(
        IReadOnlyCollection<string> positions,
        IReadOnlyCollection<string> flags,
        IReadOnlyDictionary<string, string> options,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> multiOptions,
        string raw
    )
    {
        Positions = positions;
        Flags = flags;
        Options = options;
        MultiOptions = multiOptions;
        Raw = raw;
    }
}
