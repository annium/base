using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Testing;
using Xunit;
using ServiceLifetime = Annium.Core.DependencyInjection.ServiceLifetime;

namespace Annium.Tests.Testing;

/// <summary>
/// Tests for the <see cref="TestBaseExtensions"/> helpers. Each test instantiates an isolated
/// <see cref="TestBase"/> fixture, configures it, drives the async lifecycle, and asserts
/// resolution behavior.
/// </summary>
public class TestBaseExtensionsTests
{
    /// <summary>
    /// <c>RegisterMapper</c> wires the mapper into the container so <see cref="IMapper"/> is resolvable.
    /// </summary>
    [Fact]
    public async Task RegisterMapper_MakesMapperResolvable()
    {
        await using var fixture = new InnerTest();
        fixture.RegisterMapper();
        await fixture.InitializeAsync();

        var mapper = fixture.Get<IMapper>();

        (mapper != null).IsTrue();
    }

    /// <summary>
    /// <c>RegisterTestLogs</c> with no argument registers <c>TestLog&lt;T&gt;</c> as a singleton —
    /// the same instance is returned across resolutions.
    /// </summary>
    [Fact]
    public async Task RegisterTestLogs_DefaultLifetime_IsSingleton()
    {
        await using var fixture = new InnerTest();
        fixture.RegisterTestLogs();
        await fixture.InitializeAsync();

        var a = fixture.Get<TestLog<string>>();
        var b = fixture.Get<TestLog<string>>();

        ReferenceEquals(a, b).IsTrue();
    }

    /// <summary>
    /// <c>RegisterTestLogs(ServiceLifetime.Transient)</c> produces a fresh instance per resolution.
    /// </summary>
    [Fact]
    public async Task RegisterTestLogs_TransientLifetime_IsTransient()
    {
        await using var fixture = new InnerTest();
        fixture.RegisterTestLogs(ServiceLifetime.Transient);
        await fixture.InitializeAsync();

        var a = fixture.Get<TestLog<string>>();
        var b = fixture.Get<TestLog<string>>();

        ReferenceEquals(a, b).IsFalse();
    }

    /// <summary>
    /// Minimal subclass used to instantiate <see cref="TestBase"/> outside the xUnit injection flow.
    /// Implements <see cref="System.IAsyncDisposable"/> via the inherited <c>DisposeAsync</c>.
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
