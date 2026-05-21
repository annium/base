using Annium.Core.DependencyInjection;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Tests.Testing;

/// <summary>
/// Direct tests for <see cref="TestBase"/>'s public surface area: <c>GetKeyed</c> resolution
/// and <c>CreateAsyncScope</c> scope materialization. Registrations happen in the constructor
/// — xUnit.v3 drives <c>InitializeAsync</c> before each test method, after which the registration
/// window is closed.
/// </summary>
public class TestBaseTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestBaseTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public TestBaseTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.Add(new SomeService("kv")).AsKeyed<SomeService>("alpha").Singleton());
        Register(c => c.Add<SomeService>().AsSelf().Scoped());
    }

    /// <summary>
    /// A keyed singleton registered via <c>Register</c> resolves through <c>GetKeyed</c>.
    /// </summary>
    [Fact]
    public void GetKeyed_RegisteredKeyedService_Resolves()
    {
        var resolved = GetKeyed<SomeService>("alpha");

        resolved.Name.Is("kv");
    }

    /// <summary>
    /// <c>CreateAsyncScope</c> returns a real scope whose <c>ServiceProvider</c> resolves the
    /// services registered on the container.
    /// </summary>
    [Fact]
    public void CreateAsyncScope_ProvidesScope()
    {
        using var scope = CreateAsyncScope();
        var resolved = scope.ServiceProvider.GetRequiredService<SomeService>();

        resolved.Name.Is("default");
    }

    /// <summary>
    /// Trivial service used to verify registration / resolution mechanics.
    /// </summary>
    private sealed class SomeService
    {
        public SomeService()
        {
            Name = "default";
        }

        public SomeService(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
