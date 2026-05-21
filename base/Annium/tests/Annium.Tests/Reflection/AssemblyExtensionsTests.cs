using System;
using System.Reflection;
using Annium.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Reflection;

/// <summary>
/// Direct tests for the Assembly / AssemblyName friendly-name formatters — distinct from the
/// well-tested <c>Type.FriendlyName()</c>. Locks the format so a regression in diagnostics output
/// is caught immediately.
/// </summary>
public class AssemblyExtensionsTests
{
    /// <summary>
    /// <c>AssemblyExtensions.ShortName</c> returns the simple assembly name.
    /// </summary>
    [Fact]
    public void Assembly_ShortName_ReturnsSimpleName()
    {
        var assembly = typeof(object).Assembly;

        var name = assembly.ShortName();

        name.Is("System.Private.CoreLib");
    }

    /// <summary>
    /// <c>AssemblyExtensions.FriendlyName</c> forwards to the AssemblyName variant, producing
    /// the documented <c>"Name:Version"</c> shape.
    /// </summary>
    [Fact]
    public void Assembly_FriendlyName_FormatsAsNameColonVersion()
    {
        var assembly = typeof(object).Assembly;

        var friendly = assembly.FriendlyName();
        var expected = $"{assembly.GetName().Name}:{assembly.GetName().Version}";

        friendly.Is(expected);
    }

    /// <summary>
    /// <see cref="AssemblyNameExtensions.FriendlyName"/> formats the <see cref="AssemblyName"/> as
    /// <c>"Name:Version"</c>.
    /// </summary>
    [Fact]
    public void AssemblyName_FriendlyName_FormatsAsNameColonVersion()
    {
        var assemblyName = new AssemblyName("MyLib") { Version = new Version(1, 2, 3, 4) };

        assemblyName.FriendlyName().Is("MyLib:1.2.3.4");
    }
}
