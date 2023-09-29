using System;
using System.Collections.Generic;
using System.IO;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class ResolveByImplementationExtensionTests
{
    [Fact]
    public void TypeNull_Throws()
    {
        // assert
        Wrap.It(() => (null as System.Type)!.ResolveByImplementation(typeof(byte)))
            .Throws<ArgumentNullException>();
    }

    [Fact]
    public void TargetNull_Throws()
    {
        // assert
        Wrap.It(() => typeof(byte).ResolveByImplementation((null as System.Type)!))
            .Throws<ArgumentNullException>();
    }

    [Fact]
    public void Defined_Assignable_ReturnsType()
    {
        // assert
        typeof(MemoryStream)
            .ResolveByImplementation(typeof(Stream))
            .Is(typeof(MemoryStream));
    }

    [Fact]
    public void Defined_NotAssignable_ReturnsType()
    {
        // assert
        typeof(FileInfo)
            .ResolveByImplementation(typeof(Stream))
            .IsDefault();
    }

    [Fact]
    public void Generic_NotResolved_ReturnsNull()
    {
        // assert
        typeof(List<>)
            .ResolveByImplementation(typeof(Stream))
            .IsDefault();
    }

    [Fact]
    public void GenericParameter_ReturnsParameter()
    {
        // assert
        typeof(List<>).GetGenericArguments()[0]
            .ResolveByImplementation(typeof(int))
            .Is(typeof(int));
    }

    [Fact]
    public void GenericType_ReturnsType()
    {
        // assert
        typeof(List<>)
            .ResolveByImplementation(typeof(IEnumerable<int>))
            .Is(typeof(List<int>));
    }
}