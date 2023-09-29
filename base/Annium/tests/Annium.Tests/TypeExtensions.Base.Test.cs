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
}