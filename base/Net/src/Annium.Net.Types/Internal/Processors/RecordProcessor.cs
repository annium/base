using Annium.Logging;
using Annium.Net.Types.Internal.Helpers;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class RecordProcessor : IProcessor
{
    public ILogger Logger { get; }

    public RecordProcessor(ILogger logger)
    {
        Logger = logger;
    }

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