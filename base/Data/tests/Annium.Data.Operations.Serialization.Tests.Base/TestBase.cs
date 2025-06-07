using Xunit;

namespace Annium.Data.Operations.Serialization.Tests.Base;

/// <summary>
/// Base class for serialization tests providing common test infrastructure.
/// </summary>
public abstract class TestBase : Testing.TestBase
{
    /// <summary>
    /// Initializes a new instance of the TestBase class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }
}
