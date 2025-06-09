using Annium.Logging;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

/// <summary>
/// Processor that handles generic type parameters (T, TKey, TValue, etc.).
/// </summary>
internal class GenericParameterProcessor : IProcessor
{
    /// <summary>
    /// Gets the logger for this processor.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the GenericParameterProcessor.
    /// </summary>
    /// <param name="logger">The logger to use for this processor</param>
    public GenericParameterProcessor(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Processes a contextual type if it is a generic type parameter.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>True if the type was processed as a generic parameter, false otherwise</returns>
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsGenericParameter;
    }
}
