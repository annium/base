using System;
using Annium.Logging;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

/// <summary>
/// Referrer that creates references for nullable types (both reference types and Nullable&lt;T&gt; value types).
/// </summary>
internal class NullableReferrer : IReferrer
{
    /// <summary>
    /// Gets the logger for this referrer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the NullableReferrer.
    /// </summary>
    /// <param name="logger">The logger to use for this referrer</param>
    public NullableReferrer(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Gets a nullable type reference for the specified contextual type if nullability handling is required.
    /// </summary>
    /// <param name="type">The contextual type to get a reference for</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A nullable reference if nullability handling is needed, null otherwise</returns>
    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!type.IsValueType)
            return nullability is Nullability.Unknown or Nullability.NotNullable
                ? null
                : new NullableRef(ctx.GetRef(type, Nullability.NotNullable));

        var nullableBase = Nullable.GetUnderlyingType(type);
        return nullableBase is null
            ? null
            : new NullableRef(ctx.GetRef(nullableBase.ToContextualType(), Nullability.NotNullable));
    }
}
