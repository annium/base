using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Extensions.Arguments.Internal;

namespace Annium.Extensions.Arguments;

/// <summary>
/// Extension methods for configuring argument processing services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds argument processing services to the service container
    /// </summary>
    /// <param name="container">The service container</param>
    public static void AddArguments(this IServiceContainer container)
    {
        container.Add<IArgumentProcessor, ArgumentProcessor>().Singleton();
        container.Add<IConfigurationBuilder, ConfigurationBuilder>().Singleton();
        container.Add<IConfigurationProcessor, ConfigurationProcessor>().Singleton();
        container.Add<IHelpBuilder, HelpBuilder>().Singleton();
        container.Add<Root>().AsSelf().Singleton();

        // groups and commands
        container.AddAll().AssignableTo<CommandBase>().AsSelf().Singleton();
    }
}
