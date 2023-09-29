using Annium.Logging;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class GenericParameterProcessor : IProcessor
{
    public ILogger Logger { get; }

    public GenericParameterProcessor(ILogger logger)
    {
        Logger = logger;
    }

    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsGenericParameter;
    }
}