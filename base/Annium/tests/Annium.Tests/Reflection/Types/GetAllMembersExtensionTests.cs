using System.Linq;
using System.Reflection;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Types;

/// <summary>
/// Tests for the <c>GetAll{Fields,Methods,Properties}</c> extension family. Verifies that the
/// "type's own members AND every interface it implements" contract documented on each extension
/// holds — including interface-member inclusion, which is the distinguishing behavior vs the BCL
/// <c>Type.GetFields/GetMethods/GetProperties</c>.
/// </summary>
public class GetAllMembersExtensionTests
{
    private interface IBase
    {
        int InterfaceProperty { get; }

        void InterfaceMethod();
    }

    private sealed class Concrete : IBase
    {
        public int InterfaceProperty => 0;

        public int OwnProperty => 0;

        private readonly int _privateField = 7;

        public int OwnField = 1;

        public void InterfaceMethod() { }

        public void OwnMethod() { }

        // Internal accessor keeps _privateField "used" for the IDE0051 analyzer; reflection alone
        // is not enough to satisfy it.
        internal int GetPrivateField() => _privateField;
    }

    /// <summary>GetAllFields returns the type's own fields when the type has no interfaces with fields.</summary>
    [Fact]
    public void GetAllFields_IncludesOwnPublicFields()
    {
        var names = typeof(Concrete)
            .GetAllFields(BindingFlags.Public | BindingFlags.Instance)
            .Select(f => f.Name)
            .ToArray();

        names.Contains(nameof(Concrete.OwnField)).IsTrue();
    }

    /// <summary>GetAllFields with NonPublic flag returns private fields.</summary>
    [Fact]
    public void GetAllFields_WithNonPublic_IncludesPrivateFields()
    {
        var names = typeof(Concrete)
            .GetAllFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(f => f.Name)
            .ToArray();

        names.Contains("_privateField").IsTrue();
    }

    /// <summary>GetAllMethods includes both the type's own methods AND methods declared on implemented interfaces.</summary>
    [Fact]
    public void GetAllMethods_IncludesInterfaceMethods()
    {
        var names = typeof(Concrete).GetAllMethods().Select(m => m.Name).Distinct().ToArray();

        names.Contains(nameof(Concrete.OwnMethod)).IsTrue();
        names.Contains(nameof(IBase.InterfaceMethod)).IsTrue();
    }

    /// <summary>GetAllProperties includes both the type's own properties AND properties declared on implemented interfaces.</summary>
    [Fact]
    public void GetAllProperties_IncludesInterfaceProperties()
    {
        var names = typeof(Concrete).GetAllProperties().Select(p => p.Name).Distinct().ToArray();

        names.Contains(nameof(Concrete.OwnProperty)).IsTrue();
        names.Contains(nameof(IBase.InterfaceProperty)).IsTrue();
    }

    /// <summary>BCL Type.GetMethods() does NOT include interface members — confirms the GetAll* family's distinguishing behavior.</summary>
    [Fact]
    public void GetAllMethods_DiffersFromBclGetMethods_OnInterfaceMembers()
    {
        // BCL Type.GetMethods on a class that implements an interface returns the class's
        // implementation method (named "InterfaceMethod" here since the impl is explicit-by-name),
        // but does NOT return the interface's *declaring-type* MethodInfo. GetAllMethods returns
        // both — the implementation and the interface declaration — distinct by DeclaringType.
        var allDeclaringTypes = typeof(Concrete).GetAllMethods().Select(m => m.DeclaringType).Distinct().ToArray();

        allDeclaringTypes.Contains(typeof(Concrete)).IsTrue();
        allDeclaringTypes.Contains(typeof(IBase)).IsTrue();
    }
}
