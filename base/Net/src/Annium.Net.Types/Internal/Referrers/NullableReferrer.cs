using System;
using Annium.Logging;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class NullableReferrer : IReferrer
{
    public ILogger Logger { get; }

    public NullableReferrer(ILogger logger)
    {
        Logger = logger;
    }

    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!type.IsValueType)
            return nullability is Nullability.Unknown or Nullability.NotNullable ? null : new NullableRef(ctx.GetRef(type, Nullability.NotNullable));

        var nullableBase = Nullable.GetUnderlyingType(type);
        return nullableBase is null ? null : new NullableRef(ctx.GetRef(nullableBase.ToContextualType(), Nullability.NotNullable));
    }
}