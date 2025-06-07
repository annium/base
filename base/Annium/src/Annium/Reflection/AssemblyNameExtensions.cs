using System.Reflection;

namespace Annium.Reflection;

/// <summary>
/// Provides extension methods for working with <see cref="AssemblyName"/>.
/// </summary>
public static class AssemblyNameExtensions
{
    /// <summary>
    /// Gets the friendly name of the assembly name, formatted as "Name:Version".
    /// </summary>
    /// <param name="name">The assembly name to get the friendly name for.</param>
    /// <returns>The friendly name of the assembly name.</returns>
    public static string FriendlyName(this AssemblyName name) => $"{name.Name}:{name.Version}";
}
