using Annium.Net.Mail;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// Extension methods for configuring email services in the service container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds the email service implementation to the container as a singleton
    /// </summary>
    /// <param name="container">The service container</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddEmailService(this IServiceContainer container)
    {
        container.Add<IEmailService, EmailService>().Singleton();

        return container;
    }

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
