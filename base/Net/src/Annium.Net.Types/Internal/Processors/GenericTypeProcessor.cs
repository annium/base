using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class GenericTypeProcessor : IProcessor
{
    public ILogger Logger { get; }

    public GenericTypeProcessor(ILogger logger)
    {
        Logger = logger;
    }

    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        // handles only bound generic types
        if (!type.Type.IsGenericType || type.Type.IsGenericTypeDefinition)
            return false;

        ctx.Process(type.Type.GetPure().ToContextualType());
        var typeGenericArguments = type.GetGenericArguments();
        foreach (var arg in typeGenericArguments)
            ctx.Process(arg);

        return true;
    }
}