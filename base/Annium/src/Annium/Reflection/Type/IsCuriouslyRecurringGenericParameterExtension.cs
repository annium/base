using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class IsCuriouslyRecurringGenericParameterExtension
{
    public static bool IsCuriouslyRecurringGenericParameter(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (!type.IsGenericParameter)
            return false;

        return type.GetGenericParameterConstraints()
            .Any(constraint => constraint.GetGenericArguments().Contains(type));
    }
}