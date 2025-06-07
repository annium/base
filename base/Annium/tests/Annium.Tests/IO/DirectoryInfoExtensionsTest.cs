using System;
using System.IO;
using Annium.IO;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.IO;

/// <summary>
/// Contains unit tests for DirectoryInfo extension methods.
/// </summary>
public class DirectoryInfoExtensionsTest
{
    /// <summary>
    /// Verifies that IsAt works correctly for various directory paths.
    /// </summary>
    [Fact]
    public void IsAt_WorksCorrectly()
    {
        var root = new DirectoryInfo(Directory.GetCurrentDirectory()).Root.FullName;
        GetDir("dir").IsAt(Path.Combine(root, Guid.NewGuid().ToString())).IsFalse();
        GetDir("dir").IsAt(root).IsTrue();
        GetDir(Path.Combine(root, "xx", "..", "dir")).IsAt(root).IsTrue();
        GetDir(Path.Combine(root, "xx", "..", "dir")).IsAt(Path.Combine(root, "xxx")).IsFalse();
        GetDir(root).IsAt(root, true).IsTrue();
        GetDir(root).IsAt(root).IsFalse();
        GetDir(Path.Combine(root, "xx")).IsAt(Path.Combine(root, "xx"), true).IsTrue();
        GetDir(Path.Combine(root, "xx")).IsAt(Path.Combine(root, "xx")).IsFalse();
    }

    /// <summary>
    /// Creates a new DirectoryInfo instance for the specified path.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <returns>A new DirectoryInfo instance.</returns>
    private DirectoryInfo GetDir(string path) => new(path);
}
