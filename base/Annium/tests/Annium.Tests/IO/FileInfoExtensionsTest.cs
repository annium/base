using System;
using System.IO;
using Annium.IO;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.IO;

/// <summary>
/// Contains unit tests for FileInfo extension methods.
/// </summary>
public class FileInfoExtensionsTest
{
    /// <summary>
    /// Verifies that IsAt works correctly for various file paths.
    /// </summary>
    [Fact]
    public void IsAt_WorksCorrectly()
    {
        var root = new DirectoryInfo(Directory.GetCurrentDirectory()).Root.FullName;
        GetFile("x.txt").IsAt(Path.Combine(root, Guid.NewGuid().ToString())).IsFalse();
        GetFile("x.txt").IsAt(root).IsTrue();
        GetFile(Path.Combine(root, "xx", "..", "x.txt")).IsAt(root).IsTrue();
        GetFile(Path.Combine(root, "xx", "..", "x.txt")).IsAt(Path.Combine(root, "xxx")).IsFalse();
    }

    /// <summary>
    /// Creates a new FileInfo instance for the specified path.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <returns>A new FileInfo instance.</returns>
    private FileInfo GetFile(string path) => new(path);
}
