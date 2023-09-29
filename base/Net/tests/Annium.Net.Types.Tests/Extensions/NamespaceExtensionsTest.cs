using System;
using System.IO;
using Annium.Net.Types.Extensions;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Types.Tests.Extensions;

public class NamespaceExtensionsTest
{
    [Fact]
    public void StartsWith()
    {
        "a.b".ToNamespace().StartsWith("c").IsFalse();
        "a.b".ToNamespace().StartsWith("a").IsTrue();
        "a.b".ToNamespace().StartsWith("").IsTrue();
        "a.b".ToNamespace().StartsWith("a.b").IsTrue();
    }

    [Fact]
    public void From()
    {
        Wrap.It(() => "a.b".ToNamespace().From("c")).Throws<ArgumentException>();
        "a.b".ToNamespace().From("a").Is("b".ToNamespace());
        "a.b".ToNamespace().From("").Is("a.b".ToNamespace());
        "a.b".ToNamespace().From("a.b").Is("".ToNamespace());
    }

    [Fact]
    public void Prepend()
    {
        "a.b".ToNamespace().Prepend("c").Is("c.a.b".ToNamespace());
        "a.b".ToNamespace().Prepend("").Is("a.b".ToNamespace());
    }

    [Fact]
    public void Append()
    {
        "a.b".ToNamespace().Append("c").Is("a.b.c".ToNamespace());
        "a.b".ToNamespace().Append("").Is("a.b".ToNamespace());
    }

    [Fact]
    public void ToPath()
    {
        "a.b".ToNamespace().ToPath("c").Is(Path.Combine("c", "a", "b"));
        "a.b".ToNamespace().ToPath("").Is(Path.Combine("a", "b"));
    }
}