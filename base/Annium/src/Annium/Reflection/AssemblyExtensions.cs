using System.Reflection;

namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for working with <see cref="Assembly"/>.
/// </summary>
public static class AssemblyExtensions
{
    /// <summary>
    /// Gets the friendly name of the assembly.
    /// </summary>
    /// <param name="assembly">The assembly to get the friendly name for.</param>
    /// <returns>The friendly name of the assembly.</returns>
    public static string FriendlyName(this Assembly assembly) => assembly.GetName().FriendlyName();

    /// <summary>
    /// Gets the short name of the assembly.
    /// </summary>
    /// <param name="assembly">The assembly to get the short name for.</param>
    /// <returns>The short name of the assembly, or an empty string if the name is null.</returns>
    public static string ShortName(this Assembly assembly) => assembly.GetName().Name ?? string.Empty;
}
