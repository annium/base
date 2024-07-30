using System.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class TypeBaseExtensionsTest
{
    [Fact]
    public void DefaultValue_Ok()
    {
        typeof(int).DefaultValue().Is(0);
        typeof(string[]).DefaultValue().Is(null);
        typeof(string).DefaultValue().Is(null);
    }

    [Fact]
    public void IsScalar_Ok()
    {
        typeof(int).IsScalar().IsTrue();
        typeof(string[]).IsScalar().IsFalse();
        typeof(string).IsScalar().IsTrue();
        typeof(decimal).IsScalar().IsTrue();
        typeof(TypeAttributes).IsScalar().IsTrue();
    }
}
