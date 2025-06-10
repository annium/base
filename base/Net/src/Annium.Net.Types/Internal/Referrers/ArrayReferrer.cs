using Annium.Logging;
using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

/// <summary>
/// Referrer that creates array references for array and collection types.
/// Creates ArrayRef instances wrapping the element type reference.
/// </summary>
internal class ArrayReferrer : IReferrer
{
    /// <summary>
    /// Gets the logger for this referrer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the ArrayReferrer.
    /// </summary>
    /// <param name="logger">The logger to use for this referrer</param>
    public ArrayReferrer(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Creates an array reference for array types by wrapping the element type reference.
    /// </summary>
    /// <param name="type">The contextual type to create a reference for</param>
    /// <param name="nullability">The nullability context</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>An ArrayRef if the type is an array, null otherwise</returns>
    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!ctx.Config.IsArray(type))
            return null;

        var elementType = ArrayHelper.ResolveElementType(type);
        var valueRef = ctx.GetRef(elementType);

        return new ArrayRef(valueRef);
    }
}
