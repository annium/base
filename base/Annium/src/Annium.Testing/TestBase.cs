using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Logging.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Testing;

/// <summary>
/// Provides a base class for unit tests with dependency injection, logging, and service registration utilities.
/// Implements <see cref="IAsyncLifetime"/> so xUnit.v3 drives the async DI build through
/// <see cref="IServiceProviderBuilder.BuildAsync"/>; constructor records registrations only.
/// </summary>
public abstract class TestBase : ILogSubject, IAsyncLifetime
{
    /// <summary>
    /// Gets the logger instance for the test. Throws <see cref="InvalidOperationException"/>
    /// if accessed before <see cref="InitializeAsync"/> has completed.
    /// </summary>
    public ILogger Logger
    {
        get => field ?? throw NotInitialized();
        private set;
    }

    /// <summary>
    /// Gets the captured logs.
    /// </summary>
    public IReadOnlyList<LogMessage<DefaultLogContext>> Logs => _inMemoryLogHandler.Logs;

    /// <summary>
    /// Gets the service provider for resolving dependencies. Throws <see cref="InvalidOperationException"/>
    /// if accessed before <see cref="InitializeAsync"/> has completed.
    /// </summary>
    public IServiceProvider Provider => _sp ?? throw NotInitialized();

    /// <summary>
    /// OutputHelper for this test.
    /// </summary>
    public ITestOutputHelper OutputHelper { get; }

    /// <summary>
    /// The builder for the service provider.
    /// </summary>
    private readonly IServiceProviderBuilder _builder;

    /// <summary>
    /// InMemory log handler.
    /// </summary>
    private readonly InMemoryLogHandler<DefaultLogContext> _inMemoryLogHandler = new();

    /// <summary>
    /// The materialized service provider; null until <see cref="InitializeAsync"/> completes.
    /// </summary>
    private IKeyedServiceProvider? _sp;

    /// <summary>
    /// Flipped to <c>true</c> at the entry of <see cref="InitializeAsync"/>. Closes the
    /// registration window for any subsequent <c>Register</c> / <c>Setup</c> / <c>RegisterServicePack</c> call.
    /// </summary>
    private bool _initStarted;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging.</param>
    protected TestBase(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        _builder = new ServiceProviderFactory().CreateBuilder(new ServiceCollection());

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
        EnsureOpen();
        _builder.UseServicePack<T>();
    }

    /// <summary>
    /// Registers an async registration delegate.
    /// </summary>
    /// <param name="register">The async registration delegate.</param>
    public void Register(Func<IServiceContainer, CancellationToken, Task> register)
    {
        EnsureOpen();
        _builder.UseServicePack(new DynamicServicePack().Register((c, _, ct) => register(c, ct)));
    }

    /// <summary>
    /// Registers a sync registration action — ergonomic forwarder over the async overload.
    /// </summary>
    /// <param name="register">The registration action.</param>
    public void Register(Action<IServiceContainer> register)
    {
        EnsureOpen();
        _builder.UseServicePack(new DynamicServicePack().Register((c, _) => register(c)));
    }

    /// <summary>
    /// Registers an async setup delegate executed after service provider creation.
    /// </summary>
    /// <param name="setup">The async setup delegate.</param>
    public void Setup(Func<IServiceProvider, CancellationToken, Task> setup)
    {
        EnsureOpen();
        _builder.UseServicePack(new DynamicServicePack().Setup(setup));
    }

    /// <summary>
    /// Registers a sync setup action — ergonomic forwarder over the async overload.
    /// </summary>
    /// <param name="setup">The setup action.</param>
    public void Setup(Action<IServiceProvider> setup)
    {
        EnsureOpen();
        _builder.UseServicePack(new DynamicServicePack().Setup(setup));
    }

    /// <summary>
    /// Materializes the DI container. Subclasses may override and chain via
    /// <c>await base.InitializeAsync();</c>; after the base call returns, <see cref="Provider"/>
    /// and <see cref="Logger"/> are usable inside the subclass override.
    /// </summary>
    /// <returns>A value task representing the initialization.</returns>
    public virtual async ValueTask InitializeAsync()
    {
        _initStarted = true;
        var sp = await _builder.BuildAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
        _sp = sp;
        Logger = _sp.Resolve<ILogger>();
    }

    /// <summary>
    /// Disposes the built provider, preferring <see cref="IAsyncDisposable"/> over <see cref="IDisposable"/>.
    /// Subclasses overriding must chain <c>await base.DisposeAsync();</c> last so subclass cleanup
    /// runs before the provider is torn down.
    /// </summary>
    /// <returns>A value task representing the disposal.</returns>
    public virtual async ValueTask DisposeAsync()
    {
        switch (_sp)
        {
            case IAsyncDisposable ad:
                await ad.DisposeAsync().ConfigureAwait(false);
                break;
            case IDisposable d:
                d.Dispose();
                break;
        }
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
        sp.UseLogging(x =>
        {
            x.ForAll().UseTestOutput();
            x.ForAll().UseInMemory(_inMemoryLogHandler);
        });
    }

    /// <summary>
    /// Ensures the registration window is still open (i.e. <see cref="InitializeAsync"/> has not started).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if registrations are attempted after init has begun.</exception>
    private void EnsureOpen()
    {
        if (_initStarted)
            throw new InvalidOperationException("TestBase registrations are frozen once InitializeAsync has begun.");
    }

    /// <summary>
    /// Builds the diagnostic exception thrown when <see cref="Provider"/> or <see cref="Logger"/>
    /// is accessed before <see cref="InitializeAsync"/> has completed.
    /// </summary>
    /// <returns>The configured <see cref="InvalidOperationException"/>.</returns>
    private static InvalidOperationException NotInitialized() =>
        new(
            "TestBase.Provider/Logger accessed before InitializeAsync completed. "
                + "Either the test class is missing IAsyncLifetime wiring (xUnit.v3 should drive this "
                + "automatically when TestBase implements it) or a subclass override of InitializeAsync "
                + "forgot to call `await base.InitializeAsync()`."
        );
}
