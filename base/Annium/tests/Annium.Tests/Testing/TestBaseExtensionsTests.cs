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
    /// <returns>A task that represents the asynchronous test.</returns>
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
    /// <returns>A task that represents the asynchronous test.</returns>
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
    /// <returns>A task that represents the asynchronous test.</returns>
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
        /// <summary>
        /// Initializes a new instance of the <see cref="InnerTest"/> class.
        /// </summary>
        public InnerTest()
            : base(new NullOutputHelper()) { }
    }

    /// <summary>
    /// Test output helper that drops output on the floor; used so the inner TestBase can build
    /// without depending on the outer xunit context.
    /// </summary>
    private sealed class NullOutputHelper : ITestOutputHelper
    {
        /// <summary>Gets an empty string; output is silently discarded.</summary>
        public string Output => string.Empty;

        /// <summary>Discards the message without writing it anywhere.</summary>
        /// <param name="message">The message to discard.</param>
        /// <returns>Nothing; the method returns void.</returns>
        public void Write(string message) { }

        /// <summary>Discards the formatted message without writing it anywhere.</summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="args">The arguments for the format string.</param>
        /// <returns>Nothing; the method returns void.</returns>
        public void Write(string format, params object[] args) { }

        /// <summary>Discards the line without writing it anywhere.</summary>
        /// <param name="message">The message to discard.</param>
        /// <returns>Nothing; the method returns void.</returns>
        public void WriteLine(string message) { }

        /// <summary>Discards the formatted line without writing it anywhere.</summary>
        /// <param name="format">The composite format string.</param>
        /// <param name="args">The arguments for the format string.</param>
        /// <returns>Nothing; the method returns void.</returns>
        public void WriteLine(string format, params object[] args) { }
    }
}
