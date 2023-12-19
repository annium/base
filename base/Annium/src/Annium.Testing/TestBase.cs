using System;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using AsyncServiceScope = Microsoft.Extensions.DependencyInjection.AsyncServiceScope;
using ServiceLifetime = Annium.Core.DependencyInjection.ServiceLifetime;

namespace Annium.Testing;

public abstract class TestBase : ILogSubject
{
    public ILogger Logger => _logger.Value;
    private bool _isBuilt;
    private readonly IServiceProviderBuilder _builder;
    private readonly Lazy<IKeyedServiceProvider> _sp;
    private readonly Lazy<ILogger> _logger;

    protected TestBase(ITestOutputHelper outputHelper)
    {
        _builder = new ServiceProviderFactory().CreateBuilder(new ServiceContainer().Collection);

        Register(container => container.Add(outputHelper).AsSelf().Singleton());
        Register(SharedRegister);
        Setup(SharedSetup);

        _sp = new Lazy<IKeyedServiceProvider>(BuildServiceProvider, true);
        _logger = new Lazy<ILogger>(Get<ILogger>, true);
    }

    public void AddServicePack<T>()
        where T : ServicePackBase, new()
    {
        _builder.UseServicePack<T>();
    }

    public void RegisterMapper()
    {
        Register(container => container.AddMapper(autoload: false));
    }

    public void RegisterTestLogs(ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        Register(container => container.Add(typeof(TestLog<>)).AsSelf().In(lifetime));
    }

    public void Register(Action<IServiceContainer> register)
    {
        EnsureNotBuilt();
        _builder.UseServicePack(new DynamicServicePack().Register((c, _) => register(c)));
    }

    public void Setup(Action<IServiceProvider> setup)
    {
        EnsureNotBuilt();
        _builder.UseServicePack(new DynamicServicePack().Setup(setup));
    }

    public AsyncServiceScope CreateAsyncScope()
    {
        return _sp.Value.CreateAsyncScope();
    }

    public T Get<T>()
        where T : notnull => _sp.Value.Resolve<T>();

    public T GetKeyed<T>(object key)
        where T : notnull => _sp.Value.ResolveKeyed<T>(key);

    private void SharedRegister(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithManagedTime().WithRelativeTime().SetDefault();
        container.AddLogging();
    }

    private void SharedSetup(IServiceProvider sp)
    {
        sp.UseLogging(x => x.UseTestOutput());
    }

    private IKeyedServiceProvider BuildServiceProvider()
    {
        EnsureNotBuilt();
        _isBuilt = true;

        return _builder.Build();
    }

    private void EnsureNotBuilt()
    {
        if (_isBuilt)
            throw new InvalidOperationException("ServiceProvider is already built");
    }
}
