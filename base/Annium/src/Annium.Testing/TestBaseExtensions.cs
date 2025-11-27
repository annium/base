using Annium.Core.Mapper;
using ServiceLifetime = Annium.Core.DependencyInjection.ServiceLifetime;

namespace Annium.Testing;

/// <summary>
/// Commonly used extensions for <see cref="TestBase"/>
/// </summary>
public static class TestBaseExtensions
{
    /// <summary>
    /// Registers the default mapper for tests.
    /// </summary>
    /// <param name="test">Test instance</param>
    public static void RegisterMapper(this TestBase test)
    {
        test.Register(container => container.AddMapper(autoload: false));
    }

    /// <summary>
    /// Registers test logs with the specified service lifetime.
    /// </summary>
    /// <param name="test">Test instance</param>
    /// <param name="lifetime">The service lifetime for the test logs.</param>
    public static void RegisterTestLogs(this TestBase test, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        test.Register(container => container.Add(typeof(TestLog<>)).AsSelf().In(lifetime));
    }
}
