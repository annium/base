using Annium.Logging;
using Annium.Net.Types.Internal.Helpers;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class ArrayProcessor : IProcessor
{
    public ILogger Logger { get; }

    public ArrayProcessor(ILogger logger)
    {
        Logger = logger;
    }

    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!ctx.Config.IsArray(type))
            return false;

        var elementType = ArrayHelper.ResolveElementType(type);
        ctx.Process(elementType);

        return true;
    }
}