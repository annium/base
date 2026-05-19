using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Testing;

/// <summary>
/// Tests for <see cref="TestValueExtensions.WrapWithExpression{T}"/> covering the branches the
/// private <c>Stringify</c> helper takes: <see cref="System.Type"/>, empty enumerable, non-empty
/// enumerable, null value, and string passthrough.
/// </summary>
public class TestValueExtensionsTests
{
    /// <summary>
    /// A <see cref="System.Type"/> value emits its FriendlyName rather than the runtime
    /// <see cref="object.ToString"/>. Detects a regression that drops the special Type branch.
    /// </summary>
    [Fact]
    public void WrapWithExpression_TypeValue_EmitsFullyQualifiedFriendlyName()
    {
        var value = typeof(List<int>);

        var result = value.WrapWithExpression("expr");

        // FriendlyName renders this type as `List<int>` (primitives use their C# alias) — the
        // expression text differs, so WrapWithExpression returns `"expr (List<int>)"`.
        result.Is("expr (List<int>)");
    }

    /// <summary>
    /// An empty enumerable renders as <c>[]</c>.
    /// </summary>
    [Fact]
    public void WrapWithExpression_EmptyEnumerable_ReturnsEmptyBrackets()
    {
        var value = new List<int>();

        var result = value.WrapWithExpression("expr");

        result.Is("expr ([])");
    }

    /// <summary>
    /// A non-empty enumerable renders as <c>[a, b, c]</c> in order.
    /// </summary>
    [Fact]
    public void WrapWithExpression_NonEmptyEnumerable_ReturnsBracketedElements()
    {
        var value = new[] { 1, 2, 3 };

        var result = value.WrapWithExpression("expr");

        result.Is("expr ([1, 2, 3])");
    }

    /// <summary>
    /// A null value renders as the literal string <c>null</c>.
    /// </summary>
    [Fact]
    public void WrapWithExpression_NullValue_ReturnsNullLiteral()
    {
        string? value = null;

        var result = value.WrapWithExpression("expr");

        result.Is("expr (null)");
    }

    /// <summary>
    /// When the value's string form equals the expression, <c>WrapWithExpression</c> collapses
    /// to a single rendering (no <c>"expr (expr)"</c> duplication).
    /// </summary>
    [Fact]
    public void WrapWithExpression_ValueEqualsExpression_CollapsesToSingleRendering()
    {
        // Use a scalar where ToString() round-trips to the expression text exactly.
        var value = 42;

        var result = value.WrapWithExpression("42");

        result.Is("42");
    }
}
