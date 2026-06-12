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
    /// <summary>
    /// Minimal interface whose members are expected to appear in <c>GetAllMethods</c> / <c>GetAllProperties</c>
    /// results when queried on a concrete implementing type.
    /// </summary>
    private interface IBase
    {
        /// <summary>
        /// Property declared on the interface; expected in <c>GetAllProperties</c> results for <see cref="Concrete"/>.
        /// </summary>
        int InterfaceProperty { get; }

        /// <summary>
        /// Method declared on the interface; expected in <c>GetAllMethods</c> results for <see cref="Concrete"/>.
        /// </summary>
        /// <returns>Nothing — void method used purely to test member discovery.</returns>
        void InterfaceMethod();
    }

    /// <summary>
    /// Concrete implementation of <see cref="IBase"/> used as the subject type for reflection
    /// extension tests. Combines interface members with its own public/private fields, properties,
    /// and methods to cover the full member-discovery matrix.
    /// </summary>
    private sealed class Concrete : IBase
    {
        /// <summary>
        /// Implements <see cref="IBase.InterfaceProperty"/>; expected in <c>GetAllProperties</c> results.
        /// </summary>
        public int InterfaceProperty => 0;

        /// <summary>
        /// Own property not declared on any interface; expected in <c>GetAllProperties</c> results.
        /// </summary>
        public int OwnProperty => 0;

        /// <summary>
        /// Private backing field; expected when <c>GetAllFields</c> is called with <c>NonPublic</c> binding.
        /// </summary>
        private readonly int _privateField = 7;

        /// <summary>
        /// Public field declared directly on this type; expected in <c>GetAllFields</c> results.
        /// </summary>
        public int OwnField = 1;

        /// <summary>
        /// Implements <see cref="IBase.InterfaceMethod"/>; expected in <c>GetAllMethods</c> results.
        /// </summary>
        /// <returns>Nothing — void method used purely to test member discovery.</returns>
        public void InterfaceMethod() { }

        /// <summary>
        /// Own method not declared on any interface; expected in <c>GetAllMethods</c> results.
        /// </summary>
        /// <returns>Nothing — void method used purely to test member discovery.</returns>
        public void OwnMethod() { }

        // Internal accessor keeps _privateField "used" for the IDE0051 analyzer; reflection alone
        // is not enough to satisfy it.
        /// <summary>
        /// Internal helper that returns <see cref="_privateField"/> to prevent the IDE0051 unused-field warning.
        /// </summary>
        /// <returns>The value of the private backing field.</returns>
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
