using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Xunit;

namespace Annium.Architecture.Mediator.Tests;

/// <summary>
/// Base class for mediator tests with common setup functionality.
/// </summary>
public class TestBase : Testing.TestBase
{
    public TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Registers the mediator with the specified configuration.
    /// </summary>
    /// <param name="configure">The configuration action to apply.</param>
    protected void RegisterMediator(Action<MediatorConfiguration> configure) =>
        Register(container =>
        {
            container.AddLocalization(opts => opts.UseInMemoryStorage());

            container.AddComposition();
            container.AddValidation();

            container.AddMediatorConfiguration(configure);
            container.AddMediator();
        });
}
