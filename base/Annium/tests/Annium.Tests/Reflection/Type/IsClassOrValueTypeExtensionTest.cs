using System;
using System.IO;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class IsConstructableExtensionTest
{
    [Fact]
    public void OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as System.Type)!.IsConstructable()).Throws<ArgumentNullException>();
    }

    [Fact]
    public void Class_Works()
    {
        // assert
        typeof(object).IsConstructable().IsTrue();
        typeof(FileInfo).IsConstructable().IsTrue();
        typeof(Stream).IsConstructable().IsFalse();
    }

    [Fact]
    public void Struct_Works()
    {
        // assert
        typeof(long).IsConstructable().IsTrue();
        typeof(ValueTuple<>).IsConstructable().IsTrue();
    }
}