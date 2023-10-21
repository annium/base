using Xunit.Abstractions;

namespace Annium.Data.Operations.Serialization.Tests.Base;

public abstract class TestBase : Testing.TestBase
{
    protected TestBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }
}