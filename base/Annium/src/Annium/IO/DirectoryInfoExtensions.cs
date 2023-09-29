using System;
using System.IO;

namespace Annium.IO;

public static class DirectoryInfoExtensions
{
    public static bool IsAt(this DirectoryInfo value, string root, bool self = false)
    {
        return value.IsAt(new DirectoryInfo(root), self);
    }

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