using System;
using System.IO;

namespace Annium.IO;

/// <summary>
/// Provides extension methods for working with <see cref="DirectoryInfo"/> objects.
/// </summary>
public static class DirectoryInfoExtensions
{
    /// <summary>
    /// Determines whether a directory is located at or under a specified root directory.
    /// </summary>
    /// <param name="value">The directory to check.</param>
    /// <param name="root">The root directory path to check against.</param>
    /// <param name="self">If true, also returns true if the directory is the root directory itself.</param>
    /// <returns>true if the directory is at or under the root directory; otherwise, false.</returns>
    public static bool IsAt(this DirectoryInfo value, string root, bool self = false)
    {
        return value.IsAt(new DirectoryInfo(root), self);
    }

    /// <summary>
    /// Determines whether a directory is located at or under a specified root directory.
    /// </summary>
    /// <param name="value">The directory to check.</param>
    /// <param name="root">The root directory to check against.</param>
    /// <param name="self">If true, also returns true if the directory is the root directory itself.</param>
    /// <returns>true if the directory is at or under the root directory; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsAt(this DirectoryInfo value, DirectoryInfo root, bool self = false)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        if (self && value.FullName == root.FullName)
            return true;

        var rootPath = root.FullName.EndsWith(Path.DirectorySeparatorChar)
            ? root.FullName
            : $"{root.FullName}{Path.DirectorySeparatorChar}";

        return value.FullName.Length > rootPath.Length && value.FullName.StartsWith(rootPath);
    }
}
