using System;

namespace Annium.Extensions.Arguments;

/// <summary>
/// Raised when a command line cannot be parsed: a flag repeated, or a token that is neither a position, a
/// flag nor an option with a value.
/// </summary>
/// <remarks>
/// Parsing failures used to be reported as a bare <see cref="Exception"/>, which a caller could only catch
/// by catching everything. A command line comes from a person, so the failure is expected rather than
/// exceptional, and worth its own type for a CLI to turn into a usage message.
/// </remarks>
public sealed class ArgumentParseException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentParseException"/> class.
    /// </summary>
    /// <param name="message">Description of what could not be parsed.</param>
    public ArgumentParseException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentParseException"/> class.
    /// </summary>
    /// <param name="message">Description of what could not be parsed.</param>
    /// <param name="innerException">The failure that made the value unparsable.</param>
    public ArgumentParseException(string message, Exception innerException)
        : base(message, innerException) { }
}
