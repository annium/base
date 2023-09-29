using System;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Xunit.Abstractions;

namespace Annium.Testing.Lib;

public abstract class TestBase : ILogSubject
{
    public ILogger Logger => _logger.Value;
    private bool _isBuilt;
    private readonly IServiceProviderBuilder _builder;
    private readonly Lazy<IServiceProvider> _sp;
    private readonly Lazy<ILogger> _logger;

    protected TestBase(ITestOutputHelper outputHelper)
    {
        _builder = new ServiceProviderFactory().CreateBuilder(new ServiceContainer().Collection);

        Register(container => container.Add(outputHelper).AsSelf().Singleton());
        Register(SharedRegister);
        Setup(SharedSetup);

        _sp = new Lazy<IServiceProvider>(BuildServiceProvider, true);
        _logger = new Lazy<ILogger>(Get<ILogger>, true);
    }

    protected void AddServicePack<T>()
        where T : ServicePackBase, new()
    {
        _builder.UseServicePack<T>();
    }

    protected void RegisterMapper()
    {
        Register(container => container.AddMapper(autoload: false));
    }

    protected void Register(Action<IServiceContainer> register)
    {
        EnsureNotBuilt();
        _builder.UseServicePack(new DynamicServicePack().Register((c, _) => register(c)));
    }

    protected void Setup(Action<IServiceProvider> setup)
    {
        EnsureNotBuilt();
        _builder.UseServicePack(new DynamicServicePack().Setup(setup));
    }

    protected IAsyncServiceScope CreateAsyncScope()
        => _sp.Value.CreateAsyncScope();

    protected T Get<T>()
        where T : notnull
        => _sp.Value.Resolve<T>();

    protected T GetKeyed<TKey, T>(TKey key)
        where TKey : notnull
        where T : notnull
        => _sp.Value.ResolveKeyed<TKey, T>(key);

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

    private IServiceProvider BuildServiceProvider()
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