using Annium.Logging;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class ExcludedProcessor : IProcessor
{
    public ILogger Logger { get; }

    public ExcludedProcessor(ILogger logger)
    {
        Logger = logger;
    }

    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return ctx.Config.IsExcluded(type);
    }
}