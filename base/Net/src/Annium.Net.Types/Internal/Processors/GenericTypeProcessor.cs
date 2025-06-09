using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

/// <summary>
/// Processor that handles bound generic types by processing the generic type definition and all type arguments
/// </summary>
internal class GenericTypeProcessor : IProcessor
{
    /// <summary>
    /// Logger for tracing processor operations
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the GenericTypeProcessor class
    /// </summary>
    /// <param name="logger">Logger for tracing processor operations</param>
    public GenericTypeProcessor(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Processes a bound generic type by ensuring the generic type definition and all type arguments are also processed
    /// </summary>
    /// <param name="type">The type to process</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context for managing type processing state</param>
    /// <returns>True if the type was processed (is a bound generic type), false otherwise</returns>
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
