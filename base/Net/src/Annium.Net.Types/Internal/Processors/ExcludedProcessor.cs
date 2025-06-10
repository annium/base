using Annium.Logging;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

/// <summary>
/// Processor that handles types that have been explicitly excluded from mapping.
/// </summary>
internal class ExcludedProcessor : IProcessor
{
    /// <summary>
    /// Gets the logger for this processor.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the ExcludedProcessor.
    /// </summary>
    /// <param name="logger">The logger to use for this processor</param>
    public ExcludedProcessor(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Processes a contextual type if it has been excluded from mapping.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>True if the type was excluded, false otherwise</returns>
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return ctx.Config.IsExcluded(type);
    }
}
