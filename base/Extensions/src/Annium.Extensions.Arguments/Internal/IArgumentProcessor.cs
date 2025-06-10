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
    /// <returns>A raw configuration containing parsed argument data</returns>
    RawConfiguration Compose(string[] args);
}
