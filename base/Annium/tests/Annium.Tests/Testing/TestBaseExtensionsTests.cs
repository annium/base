using Annium.Core.Mapper;
using Annium.Testing;
using Xunit;
using ServiceLifetime = Annium.Core.DependencyInjection.ServiceLifetime;

namespace Annium.Tests.Testing;

/// <summary>
/// Tests for the <see cref="TestBaseExtensions"/> helpers. Each test uses an isolated derived
/// TestBase fixture so registrations don't leak between cases.
/// </summary>
public class TestBaseExtensionsTests
{
    /// <summary>
    /// <c>RegisterMapper</c> wires the mapper into the container so <see cref="IMapper"/> is resolvable.
    /// </summary>
    [Fact]
    public void RegisterMapper_MakesMapperResolvable()
    {
        var fixture = new InnerTest();
        fixture.RegisterMapper();

        var mapper = fixture.Get<IMapper>();

        (mapper != null).IsTrue();
    }

    /// <summary>
    /// <c>RegisterTestLogs</c> with no argument registers <c>TestLog&lt;T&gt;</c> as a singleton —
    /// the same instance is returned across resolutions.
    /// </summary>
    [Fact]
    public void RegisterTestLogs_DefaultLifetime_IsSingleton()
    {
        var fixture = new InnerTest();
        fixture.RegisterTestLogs();

        var a = fixture.Get<TestLog<string>>();
        var b = fixture.Get<TestLog<string>>();

        ReferenceEquals(a, b).IsTrue();
    }

    /// <summary>
    /// <c>RegisterTestLogs(ServiceLifetime.Transient)</c> produces a fresh instance per resolution.
    /// </summary>
    [Fact]
    public void RegisterTestLogs_TransientLifetime_IsTransient()
    {
        var fixture = new InnerTest();
        fixture.RegisterTestLogs(ServiceLifetime.Transient);

        var a = fixture.Get<TestLog<string>>();
        var b = fixture.Get<TestLog<string>>();

        ReferenceEquals(a, b).IsFalse();
    }

    /// <summary>
    /// Minimal subclass used to instantiate <see cref="TestBase"/>. The test cases need a real
    /// instance because <see cref="TestBaseExtensions"/> are instance-targeted helpers.
    /// </summary>
    private sealed class InnerTest : TestBase
    {
        public InnerTest()
            : base(new NullOutputHelper()) { }
    }

    /// <summary>
    /// Test output helper that drops output on the floor; used so the inner TestBase can build
    /// without depending on the outer xunit context.
    /// </summary>
    private sealed class NullOutputHelper : ITestOutputHelper
    {
        public string Output => string.Empty;

        public void Write(string message) { }

        public void Write(string format, params object[] args) { }

        public void WriteLine(string message) { }

        public void WriteLine(string format, params object[] args) { }
    }
}
