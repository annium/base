using Annium.Core.DependencyInjection;

namespace Annium.Net.Mail.Testing;

/// <summary>
/// Extension methods for registering the test email service in the service container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds a test email service implementation to the container as a singleton
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="service">The test email service instance</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddTestEmailService(this IServiceContainer container, TestEmailService service)
    {
        container.Add(service).As<IEmailService>().Singleton();

        return container;
    }
}
