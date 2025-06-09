using Annium.Logging;
using Annium.Net.Types.Internal.Helpers;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

/// <summary>
/// Processor that handles record types (types implementing IEnumerable&lt;KeyValuePair&lt;TKey, TValue&gt;&gt;).
/// </summary>
internal class RecordProcessor : IProcessor
{
    /// <summary>
    /// Gets the logger for this processor.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the RecordProcessor.
    /// </summary>
    /// <param name="logger">The logger to use for this processor</param>
    public RecordProcessor(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Processes a contextual type if it is configured as a record type.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>True if the type was processed as a record, false otherwise</returns>
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!ctx.Config.IsRecord(type))
            return false;

        var (keyType, valueType) = RecordHelper.ResolveElementType(type);
        ctx.Process(keyType);
        ctx.Process(valueType);

        return true;
    }
}
