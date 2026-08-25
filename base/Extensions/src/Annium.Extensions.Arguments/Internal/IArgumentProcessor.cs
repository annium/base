using System.Collections.Generic;

namespace Annium.Extensions.Arguments.Internal;

/// <summary>
/// Defines the contract for processing command line arguments into structured raw configuration data.
/// </summary>
internal interface IArgumentProcessor
{
    /// <summary>
    /// Processes command line arguments and composes them into a structured raw configuration.
    /// </summary>
    /// <param name="args">Array of command line arguments to process</param>
    /// <param name="flagNames">Names and aliases of the options that are flags, and so never take a value</param>
    /// <returns>A raw configuration containing parsed argument data</returns>
    /// <remarks>
    /// The flag names have to come from the caller: a flag followed by a positional argument is
    /// indistinguishable from an option and its value by shape alone, and reading it as the latter loses
    /// both the flag and the position.
    /// </remarks>
    RawConfiguration Compose(string[] args, IReadOnlyCollection<string> flagNames);
}
