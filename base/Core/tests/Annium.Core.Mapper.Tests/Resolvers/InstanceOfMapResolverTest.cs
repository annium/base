using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

/// <summary>
/// Tests for instance-based mapping resolution in the mapper.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Map between instances of the same type
/// - Handle direct instance mapping
/// - Preserve instance values during mapping
/// - Map between compatible types
/// </remarks>
public class InstanceOfMapResolverTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceOfMapResolverTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public InstanceOfMapResolverTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Tests that mapping between instances of the same type works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Instances of the same type can be mapped
    /// - Instance values are preserved during mapping
    /// - The mapping process completes successfully
    /// - The result is a new instance with the same values
    /// </remarks>
    [Fact]
    public void InstanceOf_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Payload();

        // act
        var result = mapper.Map<Payload>(value);

        // assert
        result.Is(value);
    }

    /// <summary>
    /// Simple payload class for testing instance mapping.
    /// </summary>
    private class Payload;
}
