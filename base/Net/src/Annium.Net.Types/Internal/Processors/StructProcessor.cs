using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

/// <summary>
/// Processor that handles struct types, creating struct models with their members and inheritance hierarchy.
/// </summary>
internal class StructProcessor : IProcessor
{
    /// <summary>
    /// Gets the logger for this processor.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the StructProcessor.
    /// </summary>
    /// <param name="logger">The logger to use for this processor</param>
    public StructProcessor(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Processes a contextual type, creating a struct model if it hasn't been processed yet.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="nullability">The nullability information for the type</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>True indicating the type was processed</returns>
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (ctx.IsRegistered(type.Type))
        {
            this.Trace<string>("Process {type} - skip, already registered", type.FriendlyName());
            return true;
        }

        var model = this.InitModel(type, static (ns, isAbstract, name) => new StructModel(ns, isAbstract, name));
        ctx.Register(type.Type, model);

        ProcessType(type, ctx);
        CompleteModel(type, model, ctx);

        return true;
    }

    /// <summary>
    /// Processes the type's base type, interfaces, members, and implementations.
    /// </summary>
    /// <param name="type">The contextual type to process</param>
    /// <param name="ctx">The processing context</param>
    private void ProcessType(ContextualType type, IProcessingContext ctx)
    {
        this.ProcessBaseType(type, ctx);
        this.ProcessInterfaces(type, ctx);
        this.ProcessMembers(type, ctx);
        this.ProcessImplementations(type, ctx);
    }

    /// <summary>
    /// Completes the struct model by resolving all its properties and relationships.
    /// </summary>
    /// <param name="type">The contextual type being processed</param>
    /// <param name="model">The struct model to complete</param>
    /// <param name="ctx">The processing context</param>
    private void CompleteModel(ContextualType type, StructModel model, IProcessingContext ctx)
    {
        model.SetArgs(this.ResolveGenericArguments(type, ctx));

        var baseTypeRef = this.ResolveBaseType(type, ctx);
        if (baseTypeRef is not null)
            model.SetBase(baseTypeRef);

        model.SetInterfaces(this.ResolveInterfaces(type, ctx));
        model.SetFields(this.ResolveFields(type, ctx));
    }
}
