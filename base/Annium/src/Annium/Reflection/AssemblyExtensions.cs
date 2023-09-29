using System.Reflection;

namespace Annium.Reflection;

public static class AssemblyExtensions
{
    public static string FriendlyName(this Assembly assembly) => assembly.GetName().FriendlyName();
    public static string ShortName(this Assembly assembly) => assembly.GetName().Name ?? string.Empty;
}