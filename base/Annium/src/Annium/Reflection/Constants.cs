using System.Reflection;

namespace Annium.Reflection;

/// <summary>
/// Contains constants used in reflection operations.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Binding flags that match the public surface of a type — combines <see cref="BindingFlags.Instance"/>,
    /// <see cref="BindingFlags.Static"/>, and <see cref="BindingFlags.Public"/>. Excludes non-public members.
    /// </summary>
    public static readonly BindingFlags PublicBindingFlags =
        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
}
