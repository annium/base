using System;
using Annium.Logging;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

/// <summary>
/// Processor that handles nullable types (both reference types and Nullable&lt;T&gt; value types).
/// </summary>
internal class NullableProcessor : IProcessor
{
    /// <summary>
    /// Gets the logger for this processor.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the NullableProcessor.
    /// </summary>
    /// <param name="logger">The logger to use for this processor</param>
    public NullableProcessor(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Processes a contextual type if it involves nullable handling.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>True if the type was processed for nullability, false otherwise</returns>
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!type.IsValueType)
        {
            if (nullability is Nullability.Unknown or Nullability.NotNullable)
                return false;

            ctx.Process(type, Nullability.NotNullable);

            return true;
        }

        var nullableBase = Nullable.GetUnderlyingType(type);
        if (nullableBase is null)
            return false;

        ctx.Process(nullableBase.ToContextualType(), Nullability.NotNullable);

        return true;
    }
}
