using System;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Reflection;

public static class GetAllFieldsExtension
{
    public static FieldInfo[] GetAllFields(this Type type) =>
        type.GetAllFields(Constants.DefaultBindingFlags);

    public static FieldInfo[] GetAllFields(
        this Type type,
        BindingFlags flags
    )
    {
        var info = type.GetTypeInfo();

        return info.GetFields(flags)
            .Concat(info.ImplementedInterfaces.SelectMany(x => x.GetFields(flags)))
            .ToArray();
    }
}