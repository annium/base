using System;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static partial class ResolveGenericArgumentsByImplementationExtension
{
    public static Type[]? ResolveGenericArgumentsByImplementation(this Type type, Type target)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (type.IsGenericParameter)
            return type.ResolveGenericParameterArgumentsByTarget(target);

        if (type.IsClass)
            return type.ResolveClassArgumentsByTarget(target);

        if (type.IsValueType)
            return type.ResolveStructArgumentsByByTarget(target);

        if (type.IsInterface)
            return type.ResolveInterfaceArgumentsByTarget(target);

        // otherwise - not implemented or don't know how to resolve
        throw NotImplementedException(type, target);
    }

    private static Type[]? ResolveGenericParameterArgumentsByTarget(this Type type, Type target)
    {
        if (target.IsGenericParameter)
            return type.ResolveGenericParameterArgumentsByGenericParameter(target);

        if (target.IsClass)
            return type.ResolveGenericParameterArgumentsByClass(target);

        if (target.IsValueType)
            return type.ResolveGenericParameterArgumentsByStruct(target);

        if (target.IsInterface)
            return type.ResolveGenericParameterArgumentsByInterface(target);

        throw NotImplementedException(type, target);
    }

    private static Type[]? ResolveClassArgumentsByTarget(this Type type, Type target)
    {
        if (target.IsGenericParameter)
            return type.ResolveClassArgumentsByGenericParameter(target);

        if (target.IsClass)
            return type.ResolveClassArgumentsByClass(target);

        if (target.IsValueType)
            return null;

        if (target.IsInterface)
            return type.ResolveClassArgumentsByInterface(target);

        throw NotImplementedException(type, target);
    }

    private static Type[]? ResolveStructArgumentsByByTarget(this Type type, Type target)
    {
        if (target.IsGenericParameter)
            return type.ResolveStructArgumentsByGenericParameter(target);

        if (target.IsClass)
            return null;

        if (target.IsValueType)
            return type.ResolveStructArgumentsByStruct(target);

        if (target.IsInterface)
            return type.ResolveStructArgumentsByInterface(target);

        throw NotImplementedException(type, target);
    }

    private static Type[]? ResolveInterfaceArgumentsByTarget(this Type type, Type target)
    {
        if (target.IsGenericParameter)
            return type.ResolveInterfaceArgumentsByGenericParameter(target);

        if (target.IsClass)
            return null;

        if (target.IsValueType)
            return null;

        if (target.IsInterface)
            return type.ResolveInterfaceArgumentsByInterface(target);

        throw NotImplementedException(type, target);
    }

    private static NotImplementedException NotImplementedException(Type type, Type target) =>
        new($"Can't resolve {type.Name} generic arguments by implementation {target.Name}");
}