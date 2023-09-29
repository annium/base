using System;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class GetInheritedInterfacesExtensionTests
{
    [Fact]
    public void GetInheritedInterfaces_OfNull_Throws()
    {
        // assert
        Wrap.It(() => (null as System.Type)!.GetInheritedInterfaces()).Throws<ArgumentNullException>();
    }

    [Fact]
    public void GetInheritedInterfaces_Works()
    {
        // assert
        typeof(Derived).GetInheritedInterfaces().IsEqual(new[] { typeof(IBase), typeof(IInner), typeof(IShared) });
        typeof(Base).GetInheritedInterfaces().IsEqual(new[] { typeof(IInner) });
        typeof(IBase).GetInheritedInterfaces().IsEmpty();
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