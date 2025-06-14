using System;
using Annium.Reflection;
using Annium.Testing;
using Annium.Testing.Collection;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for the GetInheritanceChain extension method.
/// </summary>
public class GetInheritanceChainExtensionTests
{
    /// <summary>
    /// Verifies that GetInheritanceChain throws ArgumentNullException when called on null.
    /// </summary>
    [Fact]
    public void GetInheritanceChain_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.GetInheritanceChain()).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that GetInheritanceChain works correctly for the root class (object).
    /// </summary>
    [Fact]
    public void GetInheritanceChain_Class_Root_Works()
    {
        // assert
        typeof(object).GetInheritanceChain().IsEmpty();
        typeof(object).GetInheritanceChain(self: true).Has(1).At(0).Is(typeof(object));
        typeof(object).GetInheritanceChain(root: true).Has(1).At(0).Is(typeof(object));
        typeof(object).GetInheritanceChain(self: true, root: true).Has(1).At(0).Is(typeof(object));
    }

    /// <summary>
    /// Verifies that GetInheritanceChain works correctly for a non-inherited class.
    /// </summary>
    [Fact]
    public void GetInheritanceChain_Class_NotInherited_Works()
    {
        // assert
        typeof(Sample).GetInheritanceChain().IsEmpty();
        typeof(Sample).GetInheritanceChain(self: true).Has(1).At(0).Is(typeof(Sample));
        typeof(Sample).GetInheritanceChain(root: true).Has(1).At(0).Is(typeof(object));
        typeof(Sample)
            .GetInheritanceChain(self: true, root: true)
            .Has(2)
            .IsEqual(new[] { typeof(Sample), typeof(object) });
    }

    /// <summary>
    /// Verifies that GetInheritanceChain works correctly for an inherited class.
    /// </summary>
    [Fact]
    public void GetInheritanceChain_Class_Inherited_Works()
    {
        // assert
        typeof(Derived).GetInheritanceChain().Has(1).At(0).Is(typeof(Sample));
        typeof(Derived).GetInheritanceChain(self: true).Has(2).IsEqual(new[] { typeof(Derived), typeof(Sample) });
        typeof(Derived).GetInheritanceChain(root: true).Has(2).IsEqual(new[] { typeof(Sample), typeof(object) });
        typeof(Derived)
            .GetInheritanceChain(self: true, root: true)
            .Has(3)
            .IsEqual(new[] { typeof(Derived), typeof(Sample), typeof(object) });
    }

    /// <summary>
    /// Verifies that GetInheritanceChain works correctly for the root struct (ValueType).
    /// </summary>
    [Fact]
    public void GetInheritanceChain_Struct_Root_Works()
    {
        // assert
        typeof(ValueType).GetInheritanceChain().IsEmpty();
        typeof(ValueType).GetInheritanceChain(self: true).Has(1).At(0).Is(typeof(ValueType));
        typeof(ValueType).GetInheritanceChain(root: true).Has(1).At(0).Is(typeof(ValueType));
        typeof(ValueType).GetInheritanceChain(self: true, root: true).Has(1).At(0).Is(typeof(ValueType));
    }

    /// <summary>
    /// Verifies that GetInheritanceChain works correctly for a struct.
    /// </summary>
    [Fact]
    public void GetInheritanceChain_Struct_Works()
    {
        // assert
        typeof(Point).GetInheritanceChain().IsEmpty();
        typeof(Point).GetInheritanceChain(self: true).Has(1).At(0).Is(typeof(Point));
        typeof(Point).GetInheritanceChain(root: true).Has(1).At(0).Is(typeof(ValueType));
        typeof(Point)
            .GetInheritanceChain(self: true, root: true)
            .Has(2)
            .IsEqual(new[] { typeof(Point), typeof(ValueType) });
    }

    /// <summary>
    /// Verifies that GetInheritanceChain returns an empty chain for unsupported types (interfaces).
    /// </summary>
    [Fact]
    public void GetInheritanceChain_UnsupportedType_ReturnsEmptyChain()
    {
        // assert
        typeof(IFace).GetInheritanceChain().IsEmpty();
    }

    /// <summary>
    /// A derived class used for testing inheritance.
    /// </summary>
    private class Derived : Sample;

    /// <summary>
    /// A base class used for testing inheritance.
    /// </summary>
    private class Sample;

    /// <summary>
    /// A struct used for testing inheritance.
    /// </summary>
    private struct Point;

    /// <summary>
    /// An interface used for testing unsupported types.
    /// </summary>
    private interface IFace;
}
