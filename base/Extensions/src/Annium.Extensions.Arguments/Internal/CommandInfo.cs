using System;
using System.Collections.Generic;

namespace Annium.Extensions.Arguments.Internal;

/// <summary>
/// Contains metadata information about a command including its identifier, description,
/// associated handler type, and configuration types used for argument parsing.
/// </summary>
/// <param name="Id">Unique identifier for the command</param>
/// <param name="Description">Human-readable description of the command's purpose</param>
/// <param name="Type">The type that handles execution of this command</param>
/// <param name="ConfigurationTypes">Collection of configuration types used for argument parsing</param>
internal sealed record CommandInfo(
    string Id,
    string Description,
    Type Type,
    IReadOnlyCollection<Type> ConfigurationTypes
);
