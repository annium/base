using Annium.Net.Types.Extensions;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Types.Tests.Models;

/// <summary>
/// Tests for namespace model functionality
/// </summary>
public class NamespaceTests
{
    /// <summary>
    /// Tests namespace equality comparison
    /// </summary>
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
