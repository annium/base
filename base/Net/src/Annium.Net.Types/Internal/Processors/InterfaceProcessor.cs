using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

/// <summary>
/// Processor that handles interface types, creating interface models with their members and inheritance hierarchy.
/// </summary>
internal class InterfaceProcessor : IProcessor
{
    /// <summary>
    /// Gets the logger for this processor.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the InterfaceProcessor.
    /// </summary>
    /// <param name="logger">The logger to use for this processor</param>
    public InterfaceProcessor(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Processes a contextual type if it is an interface, creating an interface model.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>True if the type was processed as an interface, false otherwise</returns>
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!type.Type.IsInterface)
            return false;

        if (ctx.IsRegistered(type.Type))
        {
            this.Trace<string>("Process {type} - skip, already registered", type.FriendlyName());
            return true;
        }

        var model = this.InitModel(type, static (ns, _, name) => new InterfaceModel(ns, name));
        ctx.Register(type.Type, model);

        ProcessType(type, ctx);
        CompleteModel(type, model, ctx);

        return true;
    }

    /// <summary>
    /// Processes the type's interfaces, members, and implementations.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="ctx">The processing context</param>
    private void ProcessType(ContextualType type, IProcessingContext ctx)
    {
        this.ProcessInterfaces(type, ctx);
        this.ProcessMembers(type, ctx);
        this.ProcessImplementations(type, ctx);
    }

    /// <summary>
    /// Completes the interface model by resolving all its properties and relationships.
    /// </summary>
    /// <param name="type">The contextual type being processed</param>
    /// <param name="model">The interface model to complete</param>
    /// <param name="ctx">The processing context</param>
    private void CompleteModel(ContextualType type, InterfaceModel model, IProcessingContext ctx)
    {
        model.SetArgs(this.ResolveGenericArguments(type, ctx));
        model.SetInterfaces(this.ResolveInterfaces(type, ctx));
        model.SetFields(this.ResolveFields(type, ctx));
    }
}
