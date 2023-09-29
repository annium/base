using System;
using System.IO;

namespace Annium.IO;

public static class FileInfoExtensions
{
    public static bool IsAt(this FileInfo value, string root)
    {
        return value.IsAt(new DirectoryInfo(root));
    }

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