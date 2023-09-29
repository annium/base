using Annium.Extensions.Arguments;
using Annium.Extensions.Arguments.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static void AddArguments(this IServiceContainer container)
    {
        container.Add<IArgumentProcessor, ArgumentProcessor>().Singleton();
        container.Add<IConfigurationBuilder, ConfigurationBuilder>().Singleton();
        container.Add<IConfigurationProcessor, ConfigurationProcessor>().Singleton();
        container.Add<IHelpBuilder, HelpBuilder>().Singleton();
        container.Add<Root>().AsSelf().Singleton();

        // groups and commands
        container.AddAll()
            .AssignableTo<CommandBase>()
            .AsSelf()
            .Singleton();
    }
}