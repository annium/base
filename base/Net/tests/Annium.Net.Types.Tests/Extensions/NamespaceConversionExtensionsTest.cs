using Annium.Net.Types.Extensions;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Types.Tests.Extensions;

public class NamespaceConversionExtensions
{
    [Fact]
    public void GetNamespace()
    {
        typeof(int).GetNamespace().Is(typeof(int).Namespace!.ToNamespace());
    }

    [Fact]
    public void ToNamespaceString()
    {
        new[] { "a", "b" }.ToNamespaceString().Is("a.b");
    }
}