using System;
using Annium.Reflection;
using Annium.Testing;
using Annium.Testing.Collection;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for GetInheritedInterfaces extension method.
/// </summary>
public class GetInheritedInterfacesExtensionTests
{
    /// <summary>
    /// Verifies that GetInheritedInterfaces throws when called on null.
    /// </summary>
    [Fact]
    public void GetInheritedInterfaces_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.GetInheritedInterfaces()).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that GetInheritedInterfaces works for various types.
    /// </summary>
    [Fact]
    public void GetInheritedInterfaces_Works()
    {
        // assert
        typeof(Derived).GetInheritedInterfaces().IsEqual(new[] { typeof(IBase), typeof(IInner), typeof(IShared) });
        typeof(Base).GetInheritedInterfaces().IsEqual(new[] { typeof(IInner) });
        typeof(IBase).GetInheritedInterfaces().IsEmpty();
    }

    /// <summary>
    /// A derived class used for testing inherited interfaces.
    /// </summary>
    private class Derived : Base, IDerived;

    /// <summary>
    /// A base class used for testing inherited interfaces.
    /// </summary>
    private class Base : IBase, IShared;

    /// <summary>
    /// An interface used for testing direct inheritance.
    /// </summary>
    private interface IDerived;

    /// <summary>
    /// An interface used for testing base inheritance.
    /// </summary>
    private interface IBase : IInner;

    /// <summary>
    /// An interface used for testing shared inheritance.
    /// </summary>
    private interface IShared;

    /// <summary>
    /// An interface used for testing inner inheritance.
    /// </summary>
    private interface IInner;
}
