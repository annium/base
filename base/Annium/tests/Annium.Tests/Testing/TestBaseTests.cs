using System;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Tests.Testing;

/// <summary>
/// Direct tests for <see cref="TestBase"/>'s public surface area: <c>Register</c> /
/// <c>EnsureNotBuilt</c> guard, <c>GetKeyed</c>, and <c>CreateAsyncScope</c>.
/// </summary>
public class TestBaseTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestBaseTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public TestBaseTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Once <c>Provider</c> has been resolved, subsequent <c>Register</c> calls must throw
    /// <see cref="InvalidOperationException"/>. The guard is critical: silent acceptance of late
    /// registrations would produce a "registered but unresolvable" state for downstream services.
    /// </summary>
    [Fact]
    public void Register_AfterProviderBuilt_ThrowsInvalidOperationException()
    {
        // Trigger the lazy provider build.
        _ = Provider;

        Wrap.It(() => Register(c => c.Add<SomeService>().AsSelf().Singleton())).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// A keyed singleton registered via <c>Register</c> resolves through <c>GetKeyed</c>.
    /// </summary>
    [Fact]
    public void GetKeyed_RegisteredKeyedService_Resolves()
    {
        const string key = "alpha";
        Register(c => c.Add(new SomeService("kv")).AsKeyed<SomeService>(key).Singleton());

        var resolved = GetKeyed<SomeService>(key);

        resolved.Name.Is("kv");
    }

    /// <summary>
    /// <c>CreateAsyncScope</c> returns a real scope whose <c>ServiceProvider</c> resolves the
    /// services registered on the container.
    /// </summary>
    [Fact]
    public void CreateAsyncScope_ProvidesScope()
    {
        Register(c => c.Add<SomeService>().AsSelf().Scoped());

        using var scope = CreateAsyncScope();
        var resolved = scope.ServiceProvider.GetRequiredService<SomeService>();

        // Assert on observable state — name comes from the parameterless ctor and round-trips
        // through DI; a regression returning a stub instance with a different name would fail here.
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
