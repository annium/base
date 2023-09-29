using System;
using System.IO;
using Annium.IO;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.IO;

public class DirectoryInfoExtensionsTest
{
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

    private DirectoryInfo GetDir(string path) => new(path);
}