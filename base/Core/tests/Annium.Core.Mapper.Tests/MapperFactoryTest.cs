using System.Reflection;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// Tests for the static <see cref="Mapper"/> assembly-scoped factory facade (GetFor / Clear).
/// </summary>
/// <remarks>
/// These exercise the process-global cache directly, so each test clears the cache in a finally
/// block to avoid leaking state into sibling tests.
/// </remarks>
public class MapperFactoryTest
{
    /// <summary>The test assembly used as the per-assembly cache key for <see cref="Mapper.GetFor"/>.</summary>
    private static readonly Assembly _assembly = typeof(MapperFactoryTest).Assembly;

    /// <summary>
    /// Tests that GetFor returns the same cached mapper instance for repeated calls with the same assembly.
    /// </summary>
    [Fact]
    public void GetFor_SameAssembly_ReturnsCachedInstance()
    {
        try
        {
            var first = Mapper.GetFor(_assembly);
            var second = Mapper.GetFor(_assembly);

            ReferenceEquals(first, second).IsTrue();
        }
        finally
        {
            Mapper.Clear();
        }
    }

    /// <summary>
    /// Tests that a mapper produced by GetFor performs a real property-by-name mapping.
    /// </summary>
    [Fact]
    public void GetFor_ReturnsWorkingMapper()
    {
        try
        {
            var mapper = Mapper.GetFor(_assembly);

            var result = mapper.Map<Target>(new Source { Value = "hello" });

            result.Value.Is("hello");
        }
        finally
        {
            Mapper.Clear();
        }
    }

    /// <summary>
    /// Tests that Clear evicts the cache so a subsequent GetFor builds a fresh mapper instance.
    /// </summary>
    [Fact]
    public void Clear_AfterGetFor_RebuildsFreshInstance()
    {
        try
        {
            var first = Mapper.GetFor(_assembly);
            Mapper.Clear();
            var second = Mapper.GetFor(_assembly);

            ReferenceEquals(first, second).IsFalse();
        }
        finally
        {
            Mapper.Clear();
        }
    }

    /// <summary>Source POCO mapped by property name.</summary>
    private class Source
    {
        /// <summary>Gets or sets the value copied to the target.</summary>
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>Target POCO mapped by property name.</summary>
    private class Target
    {
        /// <summary>Gets or sets the value copied from the source.</summary>
        public string Value { get; set; } = string.Empty;
    }
}
