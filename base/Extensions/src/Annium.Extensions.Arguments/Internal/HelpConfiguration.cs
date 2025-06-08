namespace Annium.Extensions.Arguments.Internal;

/// <summary>
/// Configuration class for capturing help-related command line options.
/// Used internally to detect when help information should be displayed.
/// </summary>
internal class HelpConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether help was requested via command line option.
    /// </summary>
    [Option]
    public bool Help { get; set; }
}
