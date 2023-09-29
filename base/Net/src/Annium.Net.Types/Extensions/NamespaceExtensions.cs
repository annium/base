using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Net.Types.Models;

namespace Annium.Net.Types.Extensions;

public static class NamespaceExtensions
{
    public static bool StartsWith(this Namespace ns, Namespace target)
    {
        if (target.Count > ns.Count)
            return false;

        for (var i = 0; i < target.Count; i++)
            if (ns[i] != target[i])
                return false;

        return true;
    }

    public static Namespace From(this Namespace ns, Namespace target)
    {
        if (!ns.StartsWith(target))
            throw new ArgumentException($"Namespace {ns} doesn't contain namespace {ns}");

        return Namespace.New(ns.Skip(target.Count).ToArray());
    }

    public static Namespace Prepend(this Namespace ns, Namespace target)
    {
        return Namespace.New(target.Concat(ns).ToArray());
    }

    public static Namespace Append(this Namespace ns, Namespace target)
    {
        return Namespace.New(ns.Concat(target).ToArray());
    }

    public static string ToPath(this Namespace ns, string basePath) => Path.Combine(basePath, Path.Combine(ns.ToArray()));

    internal static T EnsureValidNamespace<T>(this T ns)
        where T : IEnumerable<string>
    {
        if (ns.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException($"Namespace {ns.ToNamespaceString()} contains empty parts");

        return ns;
    }
}