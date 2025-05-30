using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Xunit;

namespace Annium.Architecture.Mediator.Tests;

public class TestBase : Testing.TestBase
{
    public TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

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
