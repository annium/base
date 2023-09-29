using System.Collections;
using System.Collections.Generic;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection.Type;

public class IsDerivedFromExtensionTests
{
    [Fact]
    public void IsDerivedFromExtensionTests_Works()
    {
        // assert
        typeof(bool).IsDerivedFrom(typeof(object)).IsTrue();
        typeof(IEnumerable<>).IsDerivedFrom(typeof(IEnumerable)).IsTrue();
        typeof(IEnumerable<>).IsDerivedFrom(typeof(IEnumerable<>), self: false).IsFalse();
        typeof(IEnumerable<int>).IsDerivedFrom(typeof(IEnumerable<>), self: false).IsFalse();
        typeof(IEnumerable<>).IsDerivedFrom(typeof(IEnumerable<>), self: true).IsTrue();
        typeof(IEnumerable<int>).IsDerivedFrom(typeof(IEnumerable<>), self: true).IsTrue();
        typeof(List<int>).IsDerivedFrom(typeof(IEnumerable<>)).IsTrue();
        typeof(List<int>).IsDerivedFrom(typeof(IEnumerable<int>)).IsTrue();
        typeof(List<int>).IsDerivedFrom(typeof(IEnumerable<long>)).IsFalse();
        typeof(int).IsDerivedFrom(typeof(int), self: false).IsFalse();
        typeof(int).IsDerivedFrom(typeof(int), self: true).IsTrue();
    }
}