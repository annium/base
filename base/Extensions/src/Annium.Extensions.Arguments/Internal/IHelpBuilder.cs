using System;
using System.Collections.Generic;

namespace Annium.Extensions.Arguments.Internal;

/// <summary>
/// Defines the contract for building formatted help text for commands and their arguments.
/// </summary>
internal interface IHelpBuilder
{
    /// <summary>
    /// Builds help text for a command-based application showing available commands.
    /// </summary>
    /// <param name="id">The application identifier</param>
    /// <param name="description">The application description</param>
    /// <param name="commands">Collection of available commands</param>
    /// <returns>Formatted help text showing usage and available commands</returns>
    string BuildHelp(string id, string description, IReadOnlyCollection<CommandInfo> commands);

    /// <summary>
    /// Builds help text for a single command showing usage and argument details.
    /// </summary>
    /// <param name="id">The command identifier</param>
    /// <param name="description">The command description</param>
    /// <param name="configurationTypes">Array of configuration types to analyze for argument documentation</param>
    /// <returns>Formatted help text showing usage, positions, and options</returns>
    string BuildHelp(string id, string description, params Type[] configurationTypes);
}
