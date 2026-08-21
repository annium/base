using System;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Tests for <see cref="GetUnboundBaseTypeExtension.GetUnboundBaseType"/> covering null-arg, no-base,
/// concrete-class, fully-bound generic, and partially-bound generic cases.
/// </summary>
public class GetUnboundBaseTypeExtensionTests
{
    /// <summary>
    /// Null argument throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void GetUnboundBaseType_NullType_Throws()
    {
        Wrap.It(() => (null as Type)!.GetUnboundBaseType()).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// <c>object</c> has no base type — returns null.
    /// </summary>
    [Fact]
    public void GetUnboundBaseType_ObjectType_ReturnsNull()
    {
        (typeof(object).GetUnboundBaseType() is null).IsTrue();
    }

    /// <summary>
    /// A concrete class's base type is returned as-is when it contains no generic parameters.
    /// </summary>
    [Fact]
    public void GetUnboundBaseType_ConcreteClass_ReturnsBase()
    {
        typeof(Derived).GetUnboundBaseType()!.Is(typeof(Base));
    }

    /// <summary>
    /// A closed generic base (no free parameters) is returned unchanged.
    /// </summary>
    [Fact]
    public void GetUnboundBaseType_ClosedGenericBase_ReturnsBaseUnchanged()
    {
        typeof(IntDerived).GetUnboundBaseType()!.Is(typeof(GenericBase<int>));
    }

    /// <summary>
    /// When the base type still contains free generic parameters (open generic derivation), the unbound
    /// definition is returned with all parameters re-bound to the unbound slot.
    /// </summary>
    [Fact]
    public void GetUnboundBaseType_OpenGeneric_ReturnsUnboundDefinition()
    {
        var result = typeof(OpenDerived<>).GetUnboundBaseType();
        result!.IsGenericTypeDefinition.IsTrue();
        result.GetGenericTypeDefinition().Is(typeof(GenericBase<>));
    }

    /// <summary>A plain non-generic base class used as a concrete base in tests.</summary>
    private class Base;

    /// <summary>A concrete class that derives from <see cref="Base"/> with no generic parameters.</summary>
    private class Derived : Base;

    /// <summary>An open generic base class used to exercise unbound-type resolution.</summary>
    /// <typeparam name="T">The free type parameter left unbound by direct subclasses.</typeparam>
    private class GenericBase<T>;

    /// <summary>A class that closes <see cref="GenericBase{T}"/> with a concrete type argument (<c>int</c>).</summary>
    private class IntDerived : GenericBase<int>;

    /// <summary>A class that inherits <see cref="GenericBase{T}"/> while keeping the type parameter open.</summary>
    /// <typeparam name="T">The type parameter forwarded unbound to <c>GenericBase</c>.</typeparam>
    private class OpenDerived<T> : GenericBase<T>;
}
