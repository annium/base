using System.Reflection;
using Annium.Core.DependencyInjection;

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
            .AddLocalization(opts => opts.UseInMemoryStorage())
            .BuildServiceProvider()
            .Resolve<IComposer<T>>();
}
