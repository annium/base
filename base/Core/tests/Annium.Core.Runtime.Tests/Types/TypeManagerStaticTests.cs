using System.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Runtime.Tests.Types;

/// <summary>
/// Tests for the static <see cref="TypeManager"/> facade that manages the per-assembly
/// singleton cache.
/// </summary>
public class TypeManagerStaticTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeManagerStaticTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public TypeManagerStaticTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Calling GetInstance twice with the same <see cref="Assembly"/> returns the exact same
    /// object reference, confirming the CacheKey identity-equality contract.
    /// </summary>
    [Fact]
    public void GetInstance_SameAssembly_ReturnsSameInstance()
    {
        // arrange
        var assembly = typeof(TypeManagerStaticTests).Assembly;

        // act
        var first = TypeManager.GetInstance(assembly);
        var second = TypeManager.GetInstance(assembly);

        // assert
        ReferenceEquals(first, second).IsTrue();
    }

    /// <summary>
    /// After <see cref="TypeManager.Release"/> removes the cached entry, a subsequent
    /// <see cref="TypeManager.GetInstance"/> call returns a freshly constructed instance
    /// that is not reference-equal to the pre-release one.
    /// </summary>
    [Fact]
    public void Release_AfterGetInstance_AllowsFreshInstance()
    {
        // Use an assembly unlikely to be shared with other concurrent tests.
        // System.Text.Json is not registered by any other test in this project.
        var assembly = typeof(System.Text.Json.JsonDocument).Assembly;

        // act
        var before = TypeManager.GetInstance(assembly);
        TypeManager.Release(assembly);
        var after = TypeManager.GetInstance(assembly);

        // assert — post-release instance must be a brand-new object
        ReferenceEquals(before, after).IsFalse();

        // cleanup — restore cache to avoid leaking state to other tests
        TypeManager.Release(assembly);
    }
}
