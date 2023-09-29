using System;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class GetOwnInterfacesExtensionTests
{
    [Fact]
    public void GetOwnInterfaces_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as System.Type)!.GetOwnInterfaces()).Throws<ArgumentNullException>();
    }

    [Fact]
    public void GetOwnInterfaces_Works()
    {
        // assert
        typeof(Derived).GetOwnInterfaces().IsEqual(new[] { typeof(IDerived) });
        typeof(Base).GetOwnInterfaces().IsEqual(new[] { typeof(IBase), typeof(IShared) });
        typeof(IBase).GetOwnInterfaces().IsEqual(new[] { typeof(IInner) });
    }

    private class Derived : Base, IDerived
    {
    }

    private class Base : IBase, IShared
    {
    }

    private interface IDerived
    {
    }

    private interface IBase : IInner
    {
    }

    private interface IShared
    {
    }

    private interface IInner
    {
    }
}