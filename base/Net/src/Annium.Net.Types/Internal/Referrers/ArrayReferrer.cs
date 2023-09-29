using Annium.Logging;
using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class ArrayReferrer : IReferrer
{
    public ILogger Logger { get; }

    public ArrayReferrer(ILogger logger)
    {
        Logger = logger;
    }

    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!ctx.Config.IsArray(type))
            return null;

        var elementType = ArrayHelper.ResolveElementType(type);
        var valueRef = ctx.GetRef(elementType);

        return new ArrayRef(valueRef);
    }
}