using Annium.Logging;
using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

/// <summary>
/// Referrer that creates references for record types (types implementing IEnumerable&lt;KeyValuePair&lt;TKey, TValue&gt;&gt;).
/// </summary>
internal class RecordReferrer : IReferrer
{
    /// <summary>
    /// Gets the logger for this referrer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the RecordReferrer.
    /// </summary>
    /// <param name="logger">The logger to use for this referrer</param>
    public RecordReferrer(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Gets a type reference for the specified contextual type if it is configured as a record type.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A record reference if the type is a record, null otherwise</returns>
    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!ctx.Config.IsRecord(type))
            return null;

        var (keyType, valueType) = RecordHelper.ResolveElementType(type);

        var keyRef = ctx.GetRef(keyType);
        var valueRef = ctx.GetRef(valueType);

        return new RecordRef(keyRef, valueRef);
    }
}
