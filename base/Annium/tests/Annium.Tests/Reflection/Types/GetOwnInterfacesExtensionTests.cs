using System;
using Annium.Reflection.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Contains unit tests for GetOwnInterfaces extension method.
/// </summary>
public class GetOwnInterfacesExtensionTests
{
    /// <summary>
    /// Verifies that GetOwnInterfaces throws when called on null.
    /// </summary>
    [Fact]
    public void GetOwnInterfaces_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as Type)!.GetOwnInterfaces()).Throws<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that GetOwnInterfaces works for various types.
    /// </summary>
    [Fact]
    public void GetOwnInterfaces_Works()
    {
        // assert
        typeof(Derived).GetOwnInterfaces().IsEqual(new[] { typeof(IDerived) });
        typeof(Base).GetOwnInterfaces().IsEqual(new[] { typeof(IBase), typeof(IShared) });
        typeof(IBase).GetOwnInterfaces().IsEqual(new[] { typeof(IInner) });
    }

    /// <summary>
    /// A derived class used for testing own interfaces.
    /// </summary>
    private class Derived : Base, IDerived;

    /// <summary>
    /// A base class used for testing own interfaces.
    /// </summary>
    private class Base : IBase, IShared;

    /// <summary>
    /// An interface used for testing direct own interface.
    /// </summary>
    private interface IDerived;

    /// <summary>
    /// An interface used for testing base own interface.
    /// </summary>
    private interface IBase : IInner;

    /// <summary>
    /// An interface used for testing shared own interface.
    /// </summary>
    private interface IShared;

    /// <summary>
    /// An interface used for testing inner own interface.
    /// </summary>
    private interface IInner;
}
