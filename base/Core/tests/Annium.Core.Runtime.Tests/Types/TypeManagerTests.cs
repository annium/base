using System.Collections.Generic;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Runtime.Tests.Types;

/// <summary>
/// Tests for the TypeManager functionality, which provides type resolution and management capabilities.
/// </summary>
/// <remarks>
/// These tests verify various aspects of type resolution:
/// - Resolution by implementation
/// - Resolution by generic type definitions
/// - Resolution by signature
/// - Resolution by key
/// - Resolution by ID
/// - Error handling for ambiguous or missing types
/// </remarks>
public class TypeManagerTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the TypeManagerTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public TypeManagerTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests the ability to check if a type has any implementations.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Can detect implementations of a base class
    /// - Can detect implementations of interfaces
    /// - Returns false for types without implementations
    /// </remarks>
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

    /// <summary>
    /// Tests the ability to get implementations of a base class.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Can retrieve all implementations of a base class
    /// - Returns implementations in the correct order
    /// - Includes all derived types
    /// </remarks>
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

    /// <summary>
    /// Tests the ability to get implementations of generic interface definitions.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Can retrieve implementations of open generic interfaces
    /// - Includes both generic and non-generic implementations
    /// - Includes struct implementations
    /// - Returns implementations in the correct order
    /// </remarks>
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

    /// <summary>
    /// Tests the ability to get implementations of generic class definitions.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Can retrieve implementations of open generic classes
    /// - Includes both generic and non-generic implementations
    /// - Returns implementations in the correct order
    /// </remarks>
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

    /// <summary>
    /// Tests the ability to resolve a type by signature from an instance.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Can resolve a type based on the properties of an instance
    /// - Correctly identifies the most specific matching type
    /// </remarks>
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

    /// <summary>
    /// Tests the ability to resolve a type by signature from a property name list.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Can resolve a type based on a list of property names
    /// - Correctly identifies the type when exact match is required
    /// </remarks>
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

    /// <summary>
    /// Tests that resolving by key throws an exception when no descendants are found.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Throws TypeResolutionException when no matching type is found
    /// - Properly handles the case of no implementations
    /// </remarks>
    [Fact]
    public void ResolveByKey_NoDescendants_Throws()
    {
        // arrange
        var manager = Get<ITypeManager>();
        var key = "key";

        // assert
        Wrap.It(() => manager.ResolveByKey(key, typeof(F))).Throws<TypeResolutionException>();
    }

    /// <summary>
    /// Tests that resolving by key throws an exception when multiple matches are found.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Throws TypeResolutionException when multiple types match the key
    /// - Properly handles ambiguous type resolution
    /// </remarks>
    [Fact]
    public void ResolveByKey_Ambiguity_Throws()
    {
        // arrange
        var manager = Get<ITypeManager>();
        var key = "F";

        // assert
        Wrap.It(() => manager.ResolveByKey(key, typeof(D))).Throws<TypeResolutionException>();
    }

    /// <summary>
    /// Tests the ability to resolve a type by key when there is a single match.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Can resolve a type based on its key
    /// - Returns the correct type when there is a single match
    /// </remarks>
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

    /// <summary>
    /// Tests the ability to resolve a type by ID.
    /// </summary>
    /// <remarks>
    /// This test is marked for removal as it uses the deprecated type ID resolution.
    /// </remarks>
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

    /// <summary>
    /// Tests the ability to resolve a type by key from an instance.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Can resolve a type based on the key property of an instance
    /// - Returns the correct type when there is a single match
    /// </remarks>
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

    /// <summary>
    /// Tests the ability to resolve a type by signature from an instance.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Can resolve a type based on the properties of an instance
    /// - Returns the correct type when there is a single match
    /// </remarks>
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

/// <summary>
/// Base class for testing type resolution.
/// </summary>
file class A;

/// <summary>
/// Derived class with a specific property for signature-based resolution.
/// </summary>
file class B : A
{
    /// <summary>
    /// Gets or sets a property specific to class B for signature-based resolution.
    /// </summary>
    public int ForB { get; set; }
}

/// <summary>
/// Derived class with a different property for signature-based resolution.
/// </summary>
file class C : A
{
    /// <summary>
    /// Gets or sets a property specific to class C for signature-based resolution.
    /// </summary>
    public int ForC { get; set; }
}

/// <summary>
/// Base class for key-based resolution testing.
/// </summary>
file class D
{
    /// <summary>
    /// Gets the type identifier used for key-based resolution.
    /// </summary>
    [ResolutionKey]
    public string Type { get; }

    protected D(string type)
    {
        Type = type;
    }
}

/// <summary>
/// Class with a specific key value for resolution testing.
/// </summary>
[ResolutionKeyValue(nameof(E))]
file class E : D
{
    public E()
        : base(nameof(E)) { }
}

/// <summary>
/// Class with a specific key value for resolution testing.
/// </summary>
[ResolutionKeyValue(nameof(F))]
file class F : D
{
    public F()
        : base(nameof(F)) { }
}

/// <summary>
/// Class with the same key value as F for testing ambiguity.
/// </summary>
[ResolutionKeyValue(nameof(F))]
file class G : D
{
    public G()
        : base(nameof(F)) { }
}

/// <summary>
/// Base class for ID-based resolution testing.
/// </summary>
file class H
{
    /// <summary>
    /// Gets the type identifier used for ID-based resolution.
    /// </summary>
    [ResolutionId]
    public string Type => GetType().GetIdString();
}

/// <summary>
/// Derived class for ID-based resolution testing.
/// </summary>
file class K : H;

/// <summary>
/// Record for ID-based resolution testing.
/// </summary>
file record L
{
    /// <summary>
    /// Gets or sets the type identifier used for ID-based resolution.
    /// </summary>
    [ResolutionId]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Generic interface for testing generic type resolution.
/// </summary>
file interface IGenericInterface<T1, T2>;

/// <summary>
/// Generic struct implementing IGenericInterface.
/// </summary>
file record struct GenericStruct<T> : IGenericInterface<T, string>;

/// <summary>
/// Generic class implementing IGenericInterface with int as second parameter.
/// </summary>
file class GenericInterfaceDemoA<T> : IGenericInterface<T, int>;

/// <summary>
/// Generic class implementing IGenericInterface with long as second parameter.
/// </summary>
file class GenericInterfaceDemoB<T> : IGenericInterface<T, long>;

/// <summary>
/// Non-generic class implementing IGenericInterface with specific types.
/// </summary>
file class GenericInterfaceDemoC : IGenericInterface<string, bool>;

/// <summary>
/// Generic base class for testing generic class resolution.
/// </summary>
file class GenericClass<T1, T2>;

/// <summary>
/// Generic class inheriting from GenericClass with int as second parameter.
/// </summary>
file class GenericClassDemoA<T> : GenericClass<T, int>;

/// <summary>
/// Generic class inheriting from GenericClass with long as second parameter.
/// </summary>
file class GenericClassDemoB<T> : GenericClass<T, long>;

/// <summary>
/// Generic class inheriting from GenericClass with bool as second parameter.
/// </summary>
file class GenericClassDemoC : GenericClass<string, bool>;
