using System;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

/// <summary>
/// A service provider returned by <see cref="IServiceContainer.BuildServiceProvider"/> that wraps the
/// underlying <see cref="Microsoft.Extensions.DependencyInjection.ServiceProvider"/> and owns its
/// disposal. Implements both <see cref="IServiceProvider"/> and <see cref="IKeyedServiceProvider"/>
/// (every resolution delegates to the wrapped provider), and both <see cref="IDisposable"/> and
/// <see cref="IAsyncDisposable"/>.
/// </summary>
/// <remarks>
/// Disposing this container is the counterpart of building it: the container's
/// <see cref="IServiceContainer.OnDisposed"/> subscribers (captured at build time) are invoked
/// <em>before</em> the wrapped provider is torn down, so subscribers can still resolve and flush
/// services that depend on the provider (e.g. logging schedulers draining buffered messages). Both
/// <see cref="IDisposable.Dispose"/> and <see cref="IAsyncDisposable.DisposeAsync"/> run the same
/// <c>OnDisposed</c> callbacks; the synchronous path blocks on them at the disposal boundary.
/// </remarks>
public interface IServiceProviderContainer : IServiceProvider, IKeyedServiceProvider, IDisposable, IAsyncDisposable;
