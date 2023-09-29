using Annium.Net.Types.Extensions;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Types.Tests.Models;

public class NamespaceTests
{
    [Fact]
    public void Equality()
    {
        var x = "a.b".ToNamespace();
        var y = "a.b".ToNamespace();
        var z = "A.b".ToNamespace();
        x.Is(y);
        x.IsNot(z);
    }
}