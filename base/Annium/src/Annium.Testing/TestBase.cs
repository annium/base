using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Logging.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AsyncServiceScope = Microsoft.Extensions.DependencyInjection.AsyncServiceScope;

namespace Annium.Testing;

/// <summary>
/// Provides a base class for unit tests with dependency injection, logging, and service registration utilities.
/// </summary>
public abstract class TestBase : ILogSubject
{
    /// <summary>
    /// Gets the logger instance for the test.
    /// </summary>
    public ILogger Logger => _logger.Value;

    /// <summary>
    /// Gets the captured logs.
    /// </summary>
    public IReadOnlyList<LogMessage<DefaultLogContext>> Logs => _inMemoryLogHandler.Logs;

    /// <summary>
    /// Gets the service provider for resolving dependencies.
    /// </summary>
    public IServiceProvider Provider => _sp.Value;

    /// <summary>
    /// OutputHelper for this test.
    /// </summary>
    public ITestOutputHelper OutputHelper { get; }

    /// <summary>
    /// The builder for the service provider.
    /// </summary>
    private readonly IServiceProviderBuilder _builder;

    /// <summary>
    /// The lazy-initialized service provider.
    /// </summary>
    private readonly Lazy<IKeyedServiceProvider> _sp;

    /// <summary>
    /// The lazy-initialized logger.
    /// </summary>
    private readonly Lazy<ILogger> _logger;

    /// <summary>
    /// InMemory log handler.
    /// </summary>
    private readonly InMemoryLogHandler<DefaultLogContext> _inMemoryLogHandler = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging.</param>
    protected TestBase(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        _builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());
        _sp = new Lazy<IKeyedServiceProvider>(BuildServiceProvider, true);
        _logger = new Lazy<ILogger>(Get<ILogger>, true);

        Register(container => container.Add(outputHelper).AsSelf().Singleton());
        Register(SharedRegister);
        Setup(SharedSetup);
    }

    /// <summary>
    /// Adds a service pack of the specified type to the service provider.
    /// </summary>
    /// <typeparam name="T">The type of the service pack.</typeparam>
    public void RegisterServicePack<T>()
        where T : ServicePackBase, new()
    {
        _builder.UseServicePack<T>();
    }

    /// <summary>
    /// Registers a custom service registration action.
    /// </summary>
    /// <param name="register">The registration action.</param>
    public void Register(Action<IServiceContainer> register)
    {
        EnsureNotBuilt();
        _builder.UseServicePack(new DynamicServicePack().Register((c, _) => register(c)));
    }

    /// <summary>
    /// Registers a custom setup action to be executed after service provider creation.
    /// </summary>
    /// <param name="setup">The setup action.</param>
    public void Setup(Action<IServiceProvider> setup)
    {
        EnsureNotBuilt();
        _builder.UseServicePack(new DynamicServicePack().Setup(setup));
    }

    /// <summary>
    /// Creates a new asynchronous service scope.
    /// </summary>
    /// <returns>An <see cref="AsyncServiceScope"/> for managing scoped services.</returns>
    public AsyncServiceScope CreateAsyncScope()
    {
        return Provider.CreateAsyncScope();
    }

    /// <summary>
    /// Resolves a service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <returns>The resolved service instance.</returns>
    public T Get<T>()
        where T : notnull => Provider.Resolve<T>();

    /// <summary>
    /// Resolves a keyed service of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <param name="key">The key for the service.</param>
    /// <returns>The resolved service instance.</returns>
    public T GetKeyed<T>(object key)
        where T : notnull => Provider.ResolveKeyed<T>(key);

    /// <summary>
    /// Registers shared services for the test container.
    /// </summary>
    /// <param name="container">The service container to register services in.</param>
    private void SharedRegister(IServiceContainer container)
    {
        container.AddInjectables();
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithManagedTime().WithRelativeTime().SetDefault();
        container.AddLogging();
    }

    /// <summary>
    /// Performs shared setup actions after the service provider is created.
    /// </summary>
    /// <param name="sp">The service provider.</param>
    private void SharedSetup(IServiceProvider sp)
    {
        sp.UseLogging(x => x.UseTestOutput().UseInMemory(_inMemoryLogHandler));
    }

    /// <summary>
    /// Builds the service provider and marks it as built.
    /// </summary>
    /// <returns>The built <see cref="IKeyedServiceProvider"/>.</returns>
    private IKeyedServiceProvider BuildServiceProvider()
    {
        EnsureNotBuilt();

        return _builder.Build();
    }

    /// <summary>
    /// Ensures that the service provider has not been built yet.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the service provider is already built.</exception>
    private void EnsureNotBuilt()
    {
        if (_sp.IsValueCreated)
            throw new InvalidOperationException("ServiceProvider is already built");
    }
}
