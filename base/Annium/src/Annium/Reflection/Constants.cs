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

    /// <summary>
    /// Binding flags that match every instance member (public + non-public). Used when the operation must
    /// observe non-public constructors / fields (e.g. default-constructor lookup), and excludes static
    /// members on purpose.
    /// </summary>
    public static readonly BindingFlags AllInstanceBindingFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
}
