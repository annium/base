using System.Reflection;

namespace Annium.Reflection;

internal static class Constants
{
    public static readonly BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
}