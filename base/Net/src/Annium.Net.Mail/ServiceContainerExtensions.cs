using Annium.Net.Mail;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddEmailService(this IServiceContainer container)
    {
        container.Add<IEmailService, EmailService>().Singleton();

        return container;
    }

    public static IServiceContainer AddTestEmailService(this IServiceContainer container, TestEmailService service)
    {
        container.Add(service).As<IEmailService>().Singleton();

        return container;
    }
}