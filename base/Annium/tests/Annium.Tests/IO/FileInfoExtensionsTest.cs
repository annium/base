using System;
using System.IO;
using Annium.IO;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.IO;

public class FileInfoExtensionsTest
{
    [Fact]
    public void IsAt_WorksCorrectly()
    {
        var root = new DirectoryInfo(Directory.GetCurrentDirectory()).Root.FullName;
        GetFile("x.txt").IsAt(Path.Combine(root, Guid.NewGuid().ToString())).IsFalse();
        GetFile("x.txt").IsAt(root).IsTrue();
        GetFile(Path.Combine(root, "xx", "..", "x.txt")).IsAt(root).IsTrue();
        GetFile(Path.Combine(root, "xx", "..", "x.txt")).IsAt(Path.Combine(root, "xxx")).IsFalse();
    }

    private FileInfo GetFile(string path) => new(path);
}