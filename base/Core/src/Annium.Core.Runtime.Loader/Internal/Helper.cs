using System.IO;

namespace Annium.Core.Runtime.Loader.Internal;

/// <summary>
/// Internal helper methods for assembly loading operations
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Constructs a DLL file path from directory and assembly name
    /// </summary>
    /// <param name="directory">The directory containing the DLL</param>
    /// <param name="name">The assembly name without extension</param>
    /// <returns>The complete path to the DLL file</returns>
    public static string ToDllPath(string directory, string name) => Path.Combine(directory, $"{name}.dll");
}
