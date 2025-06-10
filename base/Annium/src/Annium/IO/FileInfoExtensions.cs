using System;
using System.IO;

namespace Annium.IO;

/// <summary>
/// Provides extension methods for working with <see cref="FileInfo"/> objects.
/// </summary>
public static class FileInfoExtensions
{
    /// <summary>
    /// Determines whether a file is located under a specified root directory.
    /// </summary>
    /// <param name="value">The file to check.</param>
    /// <param name="root">The root directory path to check against.</param>
    /// <returns>true if the file is under the root directory; otherwise, false.</returns>
    public static bool IsAt(this FileInfo value, string root)
    {
        return value.IsAt(new DirectoryInfo(root));
    }

    /// <summary>
    /// Determines whether a file is located under a specified root directory.
    /// </summary>
    /// <param name="value">The file to check.</param>
    /// <param name="root">The root directory to check against.</param>
    /// <returns>true if the file is under the root directory; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsAt(this FileInfo value, DirectoryInfo root)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var rootPath = root.FullName.EndsWith(Path.DirectorySeparatorChar)
            ? root.FullName
            : $"{root.FullName}{Path.DirectorySeparatorChar}";

        return value.FullName.Length > rootPath.Length && value.FullName.StartsWith(rootPath);
    }
}
