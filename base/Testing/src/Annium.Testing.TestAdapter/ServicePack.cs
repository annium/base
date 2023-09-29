using System;
using Annium.Core.DependencyInjection;

namespace Annium.Testing.TestAdapter;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add(new TestConverter(Constants.ExecutorUri)).AsSelf().Singleton();
        container.Add<TestResultConverter>().AsSelf().Singleton();
    }
}