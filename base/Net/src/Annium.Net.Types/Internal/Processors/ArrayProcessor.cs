using Annium.Logging;
using Annium.Net.Types.Internal.Helpers;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

/// <summary>
/// Processor that handles array and collection types during type mapping.
/// Processes the element type of arrays to ensure complete type mapping.
/// </summary>
internal class ArrayProcessor : IProcessor
{
    /// <summary>
    /// Gets the logger for this processor.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the ArrayProcessor.
    /// </summary>
    /// <param name="logger">The logger to use for this processor</param>
    public ArrayProcessor(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Processes array types by resolving and processing their element types.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="nullability">The nullability context</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>True if the type was handled as an array, false otherwise</returns>
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!ctx.Config.IsArray(type))
            return false;

        var elementType = ArrayHelper.ResolveElementType(type);
        ctx.Process(elementType);

        return true;
    }
}
