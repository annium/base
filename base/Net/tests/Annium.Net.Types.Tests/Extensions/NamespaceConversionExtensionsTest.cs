using Annium.Net.Types.Extensions;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Types.Tests.Extensions;

/// <summary>
/// Tests for namespace conversion extension methods
/// </summary>
public class NamespaceConversionExtensions
{
    /// <summary>
    /// Tests the GetNamespace extension method
    /// </summary>
    [Fact]
    public void GetNamespace()
    {
        typeof(int).GetNamespace().Is(typeof(int).Namespace!.ToNamespace());
    }

    /// <summary>
    /// Tests the ToNamespaceString extension method
    /// </summary>
    [Fact]
    public void ToNamespaceString()
    {
        new[] { "a", "b" }.ToNamespaceString().Is("a.b");
    }
}
