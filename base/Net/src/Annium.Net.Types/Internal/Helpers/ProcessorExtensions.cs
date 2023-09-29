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

internal static class ProcessorExtensions
{
    public static TModel InitModel<TModel>(this IProcessor processor, ContextualType type, Func<Namespace, bool, string, TModel> factory)
        where TModel : IModel
    {
        var name = type.PureName();
        var model = factory(type.GetNamespace(), type.Type.IsAbstract, name);
        processor.Trace("Initialized {type} model as {model}", type.FriendlyName(), model);

        return model;
    }

    public static void ProcessImplementations(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace<string>("Process {type} implementations", type.FriendlyName());
        var implementations = ctx.GetImplementations(type);
        if (implementations.Count == 0)
        {
            processor.Trace<string>("Process {type} implementations - no implementations", type.FriendlyName());
            return;
        }

        processor.Trace("Process {type} {implementations.Count} implementation(s)", type.FriendlyName(), implementations.Count);
        foreach (var implementation in implementations)
            ctx.Process(implementation);
    }

    public static void ProcessBaseType(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace<string>("Process {type} base type", type.FriendlyName());
        if (type.BaseType is null)
        {
            processor.Trace<string>("Process {type} base type - no base type", type.FriendlyName());
            return;
        }

        processor.Trace<string, string>("Process {type} base type {baseType}", type.FriendlyName(), type.BaseType.FriendlyName());
        ctx.Process(type.BaseType);
    }

    public static void ProcessInterfaces(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace<string>("Process {type} interfaces", type.FriendlyName());
        foreach (var @interface in type.GetOwnInterfaces())
        {
            processor.Trace<string, string>("Process {type} interface {interface}", type.FriendlyName(), @interface.FriendlyName());
            ctx.Process(@interface);
        }
    }

    public static void ProcessMembers(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace<string>("Process {type} members", type.FriendlyName());
        foreach (var member in type.GetOwnMembers())
        {
            processor.Trace<string, string, string>("Process {type} member {accessorType} {member}", type.FriendlyName(), member.AccessorType.FriendlyName(), member.Name);
            ctx.Process(member.AccessorType);
        }
    }

    public static IReadOnlyList<IRef> ResolveGenericArguments(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace<string>("Resolve {type} generic argument refs", type.FriendlyName());
        var typeGenericArguments = type.GetGenericArguments();
        var genericArguments = new List<IRef>(typeGenericArguments.Length);
        foreach (var genericArgument in typeGenericArguments)
        {
            processor.Trace<string, string>("Resolve {type} generic argument {genericArgument} ref", type.FriendlyName(), genericArgument.FriendlyName());
            genericArguments.Add(ctx.GetRef(genericArgument));
        }

        return genericArguments;
    }

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
            processor.Trace<string, string>("Resolve ignore {type} base type {baseType} ref", type.FriendlyName(), type.BaseType.FriendlyName());
            return null;
        }

        processor.Trace<string, string>("Resolve {type} base type {baseType} ref", type.FriendlyName(), type.BaseType.FriendlyName());

        return ctx.GetRef(type.BaseType);
    }

    public static IReadOnlyList<IRef> ResolveInterfaces(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace<string>("Resolve {type} interface refs", type.FriendlyName());
        var interfaces = type.GetOwnInterfaces();
        var interfaceRefs = new List<IRef>(interfaces.Count);
        foreach (var @interface in interfaces)
        {
            if (ctx.Config.IsIgnored(@interface))
            {
                processor.Trace<string, string>("Resolve ignore {type} interface {interface} ref", type.FriendlyName(), @interface.FriendlyName());
                continue;
            }

            processor.Trace<string, string>("Resolve {type} interface {interface} ref", type.FriendlyName(), @interface.FriendlyName());
            interfaceRefs.Add(ctx.GetRef(@interface));
        }

        return interfaceRefs;
    }

    public static IReadOnlyList<FieldModel> ResolveFields(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace<string>("Resolve {type} field models", type.FriendlyName());
        var fields = type.GetOwnMembers()
            .Select(member =>
            {
                processor.Trace<string, string, string>("Resolve {type} member {accessorType} {member} ref", type.FriendlyName(), member.AccessorType.FriendlyName(), member.Name);
                return new FieldModel(ctx.GetRef(member.AccessorType), member.Name);
            })
            .ToArray();

        return fields;
    }
}