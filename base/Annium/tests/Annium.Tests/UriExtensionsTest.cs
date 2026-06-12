using System;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Tests for <see cref="UriExtensions"/>.
/// </summary>
public class UriExtensionsTest
{
    /// <summary>EnsureAbsolute returns the URI when it is absolute.</summary>
    [Fact]
    public void EnsureAbsolute_AbsoluteUri_Passes()
    {
        var uri = new Uri("https://example.com/path");

        var result = uri.EnsureAbsolute();

        result.Is(uri);
    }

    /// <summary>EnsureAbsolute throws when the URI is relative.</summary>
    [Fact]
    public void EnsureAbsolute_RelativeUri_Throws()
    {
        var uri = new Uri("/relative/path", UriKind.Relative);

        Wrap.It(() => uri.EnsureAbsolute()).Throws<InvalidOperationException>();
    }
}
