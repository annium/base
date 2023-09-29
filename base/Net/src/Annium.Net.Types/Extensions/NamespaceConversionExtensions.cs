using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Models;

namespace Annium.Net.Types.Extensions;

public static class NamespaceConversionExtensions
{
    public static Namespace GetNamespace(this Type type) => (type.Namespace ?? string.Empty).ToNamespace();
    public static Namespace ToNamespace(this string ns) => Namespace.New(ns.ToNamespaceArray());
    public static Namespace ToNamespace(this IEnumerable<string> ns) => Namespace.New(ns);
    public static string ToNamespaceString(this IEnumerable<string> ns) => string.Join('.', ns);

    private static string[] ToNamespaceArray(this string ns) => ns switch
    {
        "" => Array.Empty<string>(),
        _  => ns.Split('.').ToArray().EnsureValidNamespace()
    };
}