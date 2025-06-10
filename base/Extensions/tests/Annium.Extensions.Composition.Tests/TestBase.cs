using System.Reflection;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.Runtime;
using Annium.Localization.Abstractions;
using Annium.Localization.InMemory;

namespace Annium.Extensions.Composition.Tests;

/// <summary>
/// Base class for composition tests providing common test utilities
/// </summary>
public class TestBase
{
    /// <summary>
    /// Gets a composer instance for the specified type
    /// </summary>
    /// <typeparam name="T">The type to create a composer for</typeparam>
    /// <returns>An instance of IComposer for the specified type</returns>
    protected IComposer<T> GetComposer<T>()
        where T : class =>
        new ServiceContainer()
            .AddRuntime(Assembly.GetCallingAssembly())
            .AddComposition()
            .AddLocalization(opts => LocalizationOptionsExtensions.UseInMemoryStorage(opts))
            .BuildServiceProvider()
            .Resolve<IComposer<T>>();
}
