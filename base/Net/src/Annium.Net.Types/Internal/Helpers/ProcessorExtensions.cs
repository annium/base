using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Internal.Processors;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Helpers;

/// <summary>
/// Extension methods for processors that provide common functionality for type processing operations.
/// </summary>
internal static class ProcessorExtensions
{
    /// <summary>
    /// Initializes a new model for the specified type using the provided factory function.
    /// </summary>
    /// <typeparam name="TModel">The type of model to create</typeparam>
    /// <param name="processor">The processor instance</param>
    /// <param name="type">The contextual type to create a model for</param>
    /// <param name="factory">Factory function to create the model</param>
    /// <returns>The initialized model</returns>
    public static TModel InitModel<TModel>(
        this IProcessor processor,
        ContextualType type,
        Func<Namespace, bool, string, TModel> factory
    )
        where TModel : IModel
    {
        var name = type.PureName();
        var model = factory(type.GetNamespace(), type.Type.IsAbstract, name);
        processor.Trace("Initialized {type} model as {model}", type.FriendlyName(), model);

        return model;
    }

    /// <summary>
    /// Processes all implementation types for the specified contextual type.
    /// </summary>
    /// <param name="processor">The processor instance</param>
    /// <param name="type">The contextual type to process implementations for</param>
    /// <param name="ctx">The processing context</param>
    public static void ProcessImplementations(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace<string>("Process {type} implementations", type.FriendlyName());
        var implementations = ctx.GetImplementations(type);
        if (implementations.Count == 0)
        {
            processor.Trace<string>("Process {type} implementations - no implementations", type.FriendlyName());
            return;
        }

        processor.Trace(
            "Process {type} {implementations.Count} implementation(s)",
            type.FriendlyName(),
            implementations.Count
        );
        foreach (var implementation in implementations)
            ctx.Process(implementation);
    }

    /// <summary>
    /// Processes the base type of the specified contextual type if it exists.
    /// </summary>
    /// <param name="processor">The processor instance</param>
    /// <param name="type">The contextual type to process the base type for</param>
    /// <param name="ctx">The processing context</param>
    public static void ProcessBaseType(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace<string>("Process {type} base type", type.FriendlyName());
        if (type.BaseType is null)
        {
            processor.Trace<string>("Process {type} base type - no base type", type.FriendlyName());
            return;
        }

        processor.Trace<string, string>(
            "Process {type} base type {baseType}",
            type.FriendlyName(),
            type.BaseType.FriendlyName()
        );
        ctx.Process(type.BaseType);
    }

    /// <summary>
    /// Processes all interfaces implemented by the specified contextual type.
    /// </summary>
    /// <param name="processor">The processor instance</param>
    /// <param name="type">The contextual type to process interfaces for</param>
    /// <param name="ctx">The processing context</param>
    public static void ProcessInterfaces(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace<string>("Process {type} interfaces", type.FriendlyName());
        foreach (var @interface in type.GetOwnInterfaces())
        {
            processor.Trace<string, string>(
                "Process {type} interface {interface}",
                type.FriendlyName(),
                @interface.FriendlyName()
            );
            ctx.Process(@interface);
        }
    }

    /// <summary>
    /// Processes all members (properties, fields) of the specified contextual type.
    /// </summary>
    /// <param name="processor">The processor instance</param>
    /// <param name="type">The contextual type to process members for</param>
    /// <param name="ctx">The processing context</param>
    public static void ProcessMembers(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace<string>("Process {type} members", type.FriendlyName());
        foreach (var member in type.GetOwnMembers())
        {
            processor.Trace<string, string, string>(
                "Process {type} member {accessorType} {member}",
                type.FriendlyName(),
                member.AccessorType.FriendlyName(),
                member.Name
            );
            ctx.Process(member.AccessorType);
        }
    }

    /// <summary>
    /// Resolves type references for all generic arguments of the specified contextual type.
    /// </summary>
    /// <param name="processor">The processor instance</param>
    /// <param name="type">The contextual type to resolve generic arguments for</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A list of type references for the generic arguments</returns>
    public static IReadOnlyList<IRef> ResolveGenericArguments(
        this IProcessor processor,
        ContextualType type,
        IProcessingContext ctx
    )
    {
        processor.Trace<string>("Resolve {type} generic argument refs", type.FriendlyName());
        var typeGenericArguments = type.GetGenericArguments();
        var genericArguments = new List<IRef>(typeGenericArguments.Length);
        foreach (var genericArgument in typeGenericArguments)
        {
            processor.Trace<string, string>(
                "Resolve {type} generic argument {genericArgument} ref",
                type.FriendlyName(),
                genericArgument.FriendlyName()
            );
            genericArguments.Add(ctx.GetRef(genericArgument));
        }

        return genericArguments;
    }

    /// <summary>
    /// Resolves the type reference for the base type of the specified contextual type.
    /// </summary>
    /// <param name="processor">The processor instance</param>
    /// <param name="type">The contextual type to resolve the base type for</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>The type reference for the base type, or null if no base type exists or it should be ignored</returns>
    public static IRef? ResolveBaseType(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace<string>("Resolve {type} base type ref", type.FriendlyName());
        if (type.BaseType is null)
        {
            processor.Trace<string>("Resolve {type} base type ref - no base type", type.FriendlyName());
            return null;
        }

        if (ctx.Config.IsIgnored(type.BaseType))
        {
            processor.Trace<string, string>(
                "Resolve ignore {type} base type {baseType} ref",
                type.FriendlyName(),
                type.BaseType.FriendlyName()
            );
            return null;
        }

        processor.Trace<string, string>(
            "Resolve {type} base type {baseType} ref",
            type.FriendlyName(),
            type.BaseType.FriendlyName()
        );

        return ctx.GetRef(type.BaseType);
    }

    /// <summary>
    /// Resolves type references for all interfaces implemented by the specified contextual type.
    /// </summary>
    /// <param name="processor">The processor instance</param>
    /// <param name="type">The contextual type to resolve interfaces for</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A list of type references for the implemented interfaces</returns>
    public static IReadOnlyList<IRef> ResolveInterfaces(
        this IProcessor processor,
        ContextualType type,
        IProcessingContext ctx
    )
    {
        processor.Trace<string>("Resolve {type} interface refs", type.FriendlyName());
        var interfaces = type.GetOwnInterfaces();
        var interfaceRefs = new List<IRef>(interfaces.Count);
        foreach (var @interface in interfaces)
        {
            if (ctx.Config.IsIgnored(@interface))
            {
                processor.Trace<string, string>(
                    "Resolve ignore {type} interface {interface} ref",
                    type.FriendlyName(),
                    @interface.FriendlyName()
                );
                continue;
            }

            processor.Trace<string, string>(
                "Resolve {type} interface {interface} ref",
                type.FriendlyName(),
                @interface.FriendlyName()
            );
            interfaceRefs.Add(ctx.GetRef(@interface));
        }

        return interfaceRefs;
    }

    /// <summary>
    /// Resolves field models for all members of the specified contextual type.
    /// </summary>
    /// <param name="processor">The processor instance</param>
    /// <param name="type">The contextual type to resolve fields for</param>
    /// <param name="ctx">The processing context</param>
    /// <returns>A list of field models representing the type's members</returns>
    public static IReadOnlyList<FieldModel> ResolveFields(
        this IProcessor processor,
        ContextualType type,
        IProcessingContext ctx
    )
    {
        processor.Trace<string>("Resolve {type} field models", type.FriendlyName());
        var fields = type.GetOwnMembers()
            .Select(member =>
            {
                processor.Trace<string, string, string>(
                    "Resolve {type} member {accessorType} {member} ref",
                    type.FriendlyName(),
                    member.AccessorType.FriendlyName(),
                    member.Name
                );
                return new FieldModel(ctx.GetRef(member.AccessorType), member.Name);
            })
            .ToArray();

        return fields;
    }
}
