using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class StructProcessor : IProcessor
{
    public ILogger Logger { get; }

    public StructProcessor(ILogger logger)
    {
        Logger = logger;
    }

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

    private void ProcessType(ContextualType type, IProcessingContext ctx)
    {
        this.ProcessBaseType(type, ctx);
        this.ProcessInterfaces(type, ctx);
        this.ProcessMembers(type, ctx);
        this.ProcessImplementations(type, ctx);
    }

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