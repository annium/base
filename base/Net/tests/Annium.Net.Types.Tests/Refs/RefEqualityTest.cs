using System.Collections.Generic;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Types.Tests.Refs;

/// <summary>
/// Tests for equality and hash-code semantics of InterfaceRef and StructRef
/// </summary>
public class RefEqualityTest
{
    // ── InterfaceRef ──────────────────────────────────────────────────────────

    /// <summary>
    /// Two InterfaceRef instances with the same Namespace and Name and no args are equal and share a hash code
    /// </summary>
    [Fact]
    public void InterfaceRef_SameNamespaceNameEmptyArgs_AreEqualAndShareHashCode()
    {
        var a = new InterfaceRef("Foo.Bar", "IMyInterface");
        var b = new InterfaceRef("Foo.Bar", "IMyInterface");

        a.Equals(b).IsTrue();
        (a == b).IsTrue();
        a.GetHashCode().Is(b.GetHashCode());
    }

    /// <summary>
    /// InterfaceRef.Equals(null) returns false
    /// </summary>
    [Fact]
    public void InterfaceRef_EqualsNull_ReturnsFalse()
    {
        var a = new InterfaceRef("Foo.Bar", "IMyInterface");

        a.Equals(null).IsFalse();
    }

    /// <summary>
    /// Two InterfaceRef instances with the same Namespace and Name but different Args are not equal
    /// </summary>
    [Fact]
    public void InterfaceRef_DifferentArgs_AreNotEqual()
    {
        var withArg = new InterfaceRef("Foo.Bar", "IMyInterface", new BaseTypeRef("string"));
        var withoutArg = new InterfaceRef("Foo.Bar", "IMyInterface");

        withArg.Equals(withoutArg).IsFalse();
        (withArg == withoutArg).IsFalse();
    }

    /// <summary>
    /// Two InterfaceRef instances with the same Namespace, Name, and identical Args are equal and share a hash code
    /// </summary>
    [Fact]
    public void InterfaceRef_SameArgs_AreEqualAndShareHashCode()
    {
        var argA = new BaseTypeRef("string");
        var argB = new BaseTypeRef("string");

        var a = new InterfaceRef("Foo.Bar", "IMyInterface", argA);
        var b = new InterfaceRef("Foo.Bar", "IMyInterface", argB);

        a.Equals(b).IsTrue();
        (a == b).IsTrue();
        a.GetHashCode().Is(b.GetHashCode());
    }

    /// <summary>
    /// InterfaceRef.GetHashCode does not throw when an Args element is null (covers null-tolerant HashCode.Add path)
    /// </summary>
    [Fact]
    public void InterfaceRef_NullArgElement_GetHashCodeDoesNotThrow()
    {
        // construct with an explicit null element in the Args list
        var argsWithNull = new IRef?[] { null! };
        var a = new InterfaceRef("Foo.Bar", "IMyInterface", (IReadOnlyList<IRef>)argsWithNull!);

        // must not throw
        var _ = a.GetHashCode();
    }

    // ── StructRef ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Two StructRef instances with the same Namespace and Name and no args are equal and share a hash code
    /// </summary>
    [Fact]
    public void StructRef_SameNamespaceNameEmptyArgs_AreEqualAndShareHashCode()
    {
        var a = new StructRef("Foo.Bar", "MyStruct");
        var b = new StructRef("Foo.Bar", "MyStruct");

        a.Equals(b).IsTrue();
        (a == b).IsTrue();
        a.GetHashCode().Is(b.GetHashCode());
    }

    /// <summary>
    /// StructRef.Equals(null) returns false
    /// </summary>
    [Fact]
    public void StructRef_EqualsNull_ReturnsFalse()
    {
        var a = new StructRef("Foo.Bar", "MyStruct");

        a.Equals(null).IsFalse();
    }

    /// <summary>
    /// Two StructRef instances with the same Namespace and Name but different Args are not equal
    /// </summary>
    [Fact]
    public void StructRef_DifferentArgs_AreNotEqual()
    {
        var withArg = new StructRef("Foo.Bar", "MyStruct", new BaseTypeRef("int"));
        var withoutArg = new StructRef("Foo.Bar", "MyStruct");

        withArg.Equals(withoutArg).IsFalse();
        (withArg == withoutArg).IsFalse();
    }

    /// <summary>
    /// Two StructRef instances with the same Namespace, Name, and identical Args are equal and share a hash code
    /// </summary>
    [Fact]
    public void StructRef_SameArgs_AreEqualAndShareHashCode()
    {
        var argA = new BaseTypeRef("int");
        var argB = new BaseTypeRef("int");

        var a = new StructRef("Foo.Bar", "MyStruct", argA);
        var b = new StructRef("Foo.Bar", "MyStruct", argB);

        a.Equals(b).IsTrue();
        (a == b).IsTrue();
        a.GetHashCode().Is(b.GetHashCode());
    }

    /// <summary>
    /// StructRef.GetHashCode does not throw when an Args element is null (covers null-tolerant HashCode.Add path)
    /// </summary>
    [Fact]
    public void StructRef_NullArgElement_GetHashCodeDoesNotThrow()
    {
        // construct with an explicit null element in the Args list
        var argsWithNull = new IRef?[] { null! };
        var a = new StructRef("Foo.Bar", "MyStruct", (IReadOnlyList<IRef>)argsWithNull!);

        // must not throw
        var _ = a.GetHashCode();
    }
}
