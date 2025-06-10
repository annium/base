using System;
using System.Collections.Generic;
using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

/// <summary>
/// Processor that handles enumeration types, creating enum models with their names and values.
/// </summary>
internal class EnumProcessor : IProcessor
{
    /// <summary>
    /// Gets the logger for this processor.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the EnumProcessor.
    /// </summary>
    /// <param name="logger">The logger to use for this processor</param>
    public EnumProcessor(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Processes a contextual type if it is an enumeration, creating an enum model.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>True if the type was processed as an enum, false otherwise</returns>
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!type.Type.IsEnum)
            return false;

        var names = Enum.GetNames(type.Type);
        var rawValues = Enum.GetValuesAsUnderlyingType(type.Type);

        var values = new Dictionary<string, long>();
        var i = 0;
        foreach (var value in rawValues)
            values[names[i++]] = Convert.ToInt64(value);

        var model = new EnumModel(type.GetNamespace(), type.FriendlyName(), values);
        ctx.Register(type.Type, model);

        return true;
    }
}
