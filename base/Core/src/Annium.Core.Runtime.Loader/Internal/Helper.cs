using System.IO;

namespace Annium.Core.Runtime.Loader.Internal;

internal static class Helper
{
    public static string ToDllPath(string directory, string name) => Path.Combine(directory, $"{name}.dll");
}