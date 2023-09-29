using System.Collections.Generic;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Runtime.Tests.Types;

public class TypeManagerTests : TestBase
{
    public TypeManagerTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void CanResolve_Works()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // assert
        manager.HasImplementations(typeof(A)).IsTrue();
        manager.HasImplementations(typeof(B)).IsFalse();
        manager.HasImplementations(typeof(C)).IsFalse();
        manager.HasImplementations(typeof(IEnumerable<>)).IsTrue();
    }

    [Fact]
    public void GetImplementations_ForAncestors_Works()
    {
        // arrange
        var manager = Get<ITypeManager>();

        var implementations = manager.GetImplementations(typeof(A));

        // assert
        implementations.Has(2);
        implementations.At(0).Is(typeof(B));
        implementations.At(1).Is(typeof(C));
    }

    [Fact]
    public void GetImplementations_ForGenericInterfaceDefinitions_Works()
    {
        // arrange
        var manager = Get<ITypeManager>();

        var implementations = manager.GetImplementations(typeof(IGenericInterface<,>));

        // assert
        implementations.Has(4);
        implementations.At(0).Is(typeof(GenericInterfaceDemoA<>));
        implementations.At(1).Is(typeof(GenericInterfaceDemoB<>));
        implementations.At(2).Is(typeof(GenericInterfaceDemoC));
        implementations.At(3).Is(typeof(GenericStruct<>));
    }

    [Fact]
    public void GetImplementations_ForGenericClassDefinitions_Works()
    {
        // arrange
        var manager = Get<ITypeManager>();

        var implementations = manager.GetImplementations(typeof(GenericClass<,>));

        // assert
        implementations.Has(3);
        implementations.At(0).Is(typeof(GenericClassDemoA<>));
        implementations.At(1).Is(typeof(GenericClassDemoB<>));
        implementations.At(2).Is(typeof(GenericClassDemoC));
    }

    [Fact]
    public void ResolveBySignature_FromInstance_Works()
    {
        // arrange
        var manager = Get<ITypeManager>();
        var value = new { ForB = 5 };

        // act
        var result = manager.Resolve(value, typeof(A));

        // assert
        result.Is(typeof(B));
    }

    [Fact]
    public void ResolveBySignature_FromSignature_Works()
    {
        // arrange
        var manager = Get<ITypeManager>();

        // act
        var result = manager.ResolveBySignature(new[] { nameof(B.ForB) }, typeof(A), true);

        // assert
        result.Is(typeof(B));
    }

    [Fact]
    public void ResolveByKey_NoDescendants_Throws()
    {
        // arrange
        var manager = Get<ITypeManager>();
        var key = "key";

        // assert
        Wrap.It(() => manager.ResolveByKey(key, typeof(F))).Throws<TypeResolutionException>();
    }

    [Fact]
    public void ResolveByKey_Ambiguity_Throws()
    {
        // arrange
        var manager = Get<ITypeManager>();
        var key = "F";

        // assert
        Wrap.It(() => manager.ResolveByKey(key, typeof(D))).Throws<TypeResolutionException>();
    }

    [Fact]
    public void ResolveByKey_Normally_Works()
    {
        // arrange
        var manager = Get<ITypeManager>();
        var key = "E";

        // act
        var result = manager.ResolveByKey(key, typeof(D));

        // assert
        result.Is(typeof(E));
    }

    [Fact(Skip = "to be dropped with type id")]
    public void Resolve_ById_Works()
    {
        // arrange
        var manager = Get<ITypeManager>();
        var source = new K();

        // act
        var result = manager.Resolve(source, typeof(H));

        // assert
        result.Is(typeof(K));
    }

    [Fact]
    public void Resolve_ByKey_Works()
    {
        // arrange
        var manager = Get<ITypeManager>();
        object source = new E();

        // act
        var result = manager.Resolve(source, typeof(D));

        // assert
        result.Is(typeof(E));
    }

    [Fact]
    public void Resolve_BySignature_Works()
    {
        // arrange
        var manager = Get<ITypeManager>();
        object source = new B();

        // act
        var result = manager.Resolve(source, typeof(A));

        // assert
        result.Is(typeof(B));
    }
}

file class A
{
}

file class B : A
{
    public int ForB { get; set; }
}

file class C : A
{
    public int ForC { get; set; }
}

file class D
{
    [ResolutionKey]
    public string Type { get; }

    protected D(string type)
    {
        Type = type;
    }
}

[ResolutionKeyValue(nameof(E))]
file class E : D
{
    public E() : base(nameof(E))
    {
    }
}

[ResolutionKeyValue(nameof(F))]
file class F : D
{
    public F() : base(nameof(F))
    {
    }
}

[ResolutionKeyValue(nameof(F))]
// ReSharper disable once UnusedType.Local
file class G : D
{
    public G() : base(nameof(F))
    {
    }
}

file class H
{
    [ResolutionId]
    public string Type => GetType().GetIdString();
}

file class K : H
{
}

file record L
{
    [ResolutionId]
    public string Type { get; set; } = string.Empty;
}

file interface IGenericInterface<T1, T2>
{
}

file record struct GenericStruct<T> : IGenericInterface<T, string>;

file class GenericInterfaceDemoA<T> : IGenericInterface<T, int>
{
}

file class GenericInterfaceDemoB<T> : IGenericInterface<T, long>
{
}

file class GenericInterfaceDemoC : IGenericInterface<string, bool>
{
}

file class GenericClass<T1, T2>
{
}

file class GenericClassDemoA<T> : GenericClass<T, int>
{
}

file class GenericClassDemoB<T> : GenericClass<T, long>
{
}

file class GenericClassDemoC : GenericClass<string, bool>
{
}