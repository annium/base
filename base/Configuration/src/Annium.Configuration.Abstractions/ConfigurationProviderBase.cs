using System.Collections.Generic;
using System.Linq;

namespace Annium.Configuration.Abstractions;

/// <summary>
/// Base class for configuration providers that read configuration data from various sources
/// </summary>
public abstract class ConfigurationProviderBase
{
    /// <summary>
    /// Dictionary storing configuration data with key paths and string values
    /// </summary>
    protected Dictionary<string[], string> Data = new();

    /// <summary>
    /// Stack maintaining the current path context during configuration processing
    /// </summary>
    protected Stack<string> Context = new();

    /// <summary>
    /// Gets the current path as an array of strings from the context stack
    /// </summary>
    protected string[] Path => Context.Reverse().ToArray();

    /// <summary>
    /// Reads configuration data from the source and returns it as a dictionary
    /// </summary>
    /// <returns>Dictionary containing configuration keys and values</returns>
    public abstract IReadOnlyDictionary<string[], string> Read();

    /// <summary>
    /// Initializes the configuration provider by resetting data and context
    /// </summary>
    protected void Init()
    {
        Data = new Dictionary<string[], string>();
        Context = new Stack<string>();
    }
}
