using System;
using System.Collections.Generic;
using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class EnumProcessor : IProcessor
{
    public ILogger Logger { get; }

    public EnumProcessor(ILogger logger)
    {
        Logger = logger;
    }

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