using Annium.Logging;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class IgnoredProcessor : IProcessor
{
    public ILogger Logger { get; }

    public IgnoredProcessor(ILogger logger)
    {
        Logger = logger;
    }

    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return ctx.Config.IsIgnored(type);
    }
}