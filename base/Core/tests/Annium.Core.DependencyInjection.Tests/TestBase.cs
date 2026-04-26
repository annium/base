using System;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests;

/// <summary>
/// Test base for the DI package. Inherits the canonical <see cref="Annium.Testing.TestBase"/>
/// (T9 consolidation) but keeps a separate local <see cref="Container"/> so each test can
/// mutate the container directly without coupling to the inherited services
/// (runtime/time/logging) — those still live in the base class's container, isolated from
/// the local one.
/// </summary>
public class TestBase : Annium.Testing.TestBase
{
    /// <summary>
    /// Local service container the tests mutate directly via <c>Container.Add</c>.
    /// </summary>
    protected readonly ServiceContainer Container = new();

    /// <summary>
    /// The provider built from <see cref="Container"/> — populated by <see cref="Build"/>.
    /// </summary>
    private IServiceProvider _provider = default!;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="outputHelper">xunit output helper.</param>
    public TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Builds the local service provider from <see cref="Container"/>.
    /// </summary>
    protected void Build()
    {
        _provider = Container.BuildServiceProvider();
    }

    /// <summary>
    /// Resolves a service from the locally-built provider. Hides the inherited
    /// <see cref="Annium.Testing.TestBase.Get{T}"/> on purpose — DI tests resolve from
    /// their own container, not the inherited one.
    /// </summary>
    /// <typeparam name="T">Service type.</typeparam>
    /// <returns>Resolved service.</returns>
    protected new T Get<T>()
        where T : notnull
    {
        return _provider.Resolve<T>();
    }

    /// <summary>
    /// Resolves a keyed service from the locally-built provider. Hides the inherited
    /// <see cref="Annium.Testing.TestBase.GetKeyed{T}"/> on purpose for the same reason.
    /// </summary>
    /// <typeparam name="T">Service type.</typeparam>
    /// <param name="key">Service key.</param>
    /// <returns>Resolved service.</returns>
    protected new T GetKeyed<T>(object key)
        where T : notnull
    {
        return _provider.ResolveKeyed<T>(key);
    }
}
