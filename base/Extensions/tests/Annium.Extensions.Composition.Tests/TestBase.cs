using System.Reflection;
using Annium.Core.DependencyInjection;

namespace Annium.Extensions.Composition.Tests;

public class TestBase
{
    protected IComposer<T> GetComposer<T>() where T : class => new ServiceContainer()
        .AddRuntime(Assembly.GetCallingAssembly())
        .AddComposition()
        .AddLocalization(opts => opts.UseInMemoryStorage())
        .BuildServiceProvider()
        .Resolve<IComposer<T>>();
}