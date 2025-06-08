using System.Reflection;

namespace Annium.Reflection;

/// <summary>
/// Contains constants used in reflection operations.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// The default binding flags used for reflection operations, combining Instance, Static, and Public flags.
    /// </summary>
    public static readonly BindingFlags DefaultBindingFlags =
        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
}
