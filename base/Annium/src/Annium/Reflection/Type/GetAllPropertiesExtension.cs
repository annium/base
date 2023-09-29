using System;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class GetAllPropertiesExtension
{
    public static PropertyInfo[] GetAllProperties(this Type type) =>
        type.GetAllProperties(Constants.DefaultBindingFlags);

    public static PropertyInfo[] GetAllProperties(
        this Type type,
        BindingFlags flags
    )
    {
        var info = type.GetTypeInfo();

        return info.GetProperties(flags)
            .Concat(info.ImplementedInterfaces.SelectMany(x => x.GetProperties(flags)))
            .ToArray();
    }
}