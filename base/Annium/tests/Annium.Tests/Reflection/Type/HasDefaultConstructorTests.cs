using System;
using System.Collections;
using System.IO;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class HasDefaultConstructorTests
{
    [Fact]
    public void HasDefaultConstructor_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as System.Type)!.HasDefaultConstructor()).Throws<ArgumentNullException>();
    }

    [Fact]
    public void HasDefaultConstructor_Class_Works()
    {
        // assert
        typeof(object).HasDefaultConstructor().IsTrue();
        typeof(FileInfo).HasDefaultConstructor().IsFalse();
    }

    [Fact]
    public void HasDefaultConstructor_Struct_Works()
    {
        // assert
        typeof(long).HasDefaultConstructor().IsTrue();
        typeof(ValueTuple<>).HasDefaultConstructor().IsFalse();
    }

    [Fact]
    public void HasDefaultConstructor_Other_Throws()
    {
        // assert
        Wrap.It(() => typeof(IEnumerable).HasDefaultConstructor()).Throws<ArgumentException>();
    }
}