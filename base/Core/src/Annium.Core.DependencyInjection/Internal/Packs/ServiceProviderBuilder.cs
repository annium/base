using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal.Packs;

/// <summary>
/// Internal implementation of service provider builder that manages service packs and builds service providers
/// </summary>
internal class ServiceProviderBuilder : IServiceProviderBuilder
{
    /// <summary>
    /// Flag indicating whether the service provider has already been built successfully.
    /// Only flipped on the normal return path of <see cref="BuildAsync"/>; a failed build leaves the builder re-buildable.
    /// </summary>
    private bool _isAlreadyBuilt;

    /// <summary>
    /// The service container instance
    /// </summary>
    private readonly IServiceContainer _container;

    /// <summary>
    /// The collection of service packs to be configured and registered
    /// </summary>
    private readonly IList<ServicePackBase> _packs = new List<ServicePackBase>();

    /// <summary>
    /// Initializes a new instance of the ServiceProviderBuilder class with an empty service container
    /// </summary>
    public ServiceProviderBuilder()
    {
        _container = new ServiceContainer();
    }

    /// <summary>
    /// Initializes a new instance of the ServiceProviderBuilder class with the specified service collection
    /// </summary>
    /// <param name="services">The service collection to initialize the container with</param>
    public ServiceProviderBuilder(IServiceCollection services)
    {
        _container = new ServiceContainer(services);
    }

    /// <summary>
    /// Adds a service pack of the specified type to the builder if not already added
    /// </summary>
    /// <typeparam name="TServicePack">The type of service pack to add</typeparam>
    /// <returns>The current service provider builder instance</returns>
    public IServiceProviderBuilder UseServicePack<TServicePack>()
        where TServicePack : ServicePackBase, new()
    {
        if (_packs.All(e => e.GetType() != typeof(TServicePack)))
            _packs.Add(new TServicePack());

        return this;
    }

    /// <summary>
    /// Adds the specified service pack instance to the builder
    /// </summary>
    /// <param name="servicePack">The service pack instance to add</param>
    /// <returns>The current service provider builder instance</returns>
    public IServiceProviderBuilder UseServicePack(ServicePackBase servicePack)
    {
        _packs.Add(servicePack);

        return this;
    }

    /// <summary>
    /// Asynchronously builds the service provider by configuring, registering, and setting up all service packs.
    /// <para>
    /// The three-phase <see cref="ServicePackBase"/> model is preserved: <c>ConfigureAsync</c> populates
    /// a staging container, a transient provider is materialized for <c>RegisterAsync</c> (so packs can
    /// consume Configure-phase services), then a final provider is built for <c>SetupAsync</c>. The
    /// transient provider is disposed before <c>SetupAsync</c> runs to release any singletons it
    /// materialized.
    /// </para>
    /// <para>
    /// Cancellation: the supplied <paramref name="ct"/> is threaded into every pack's
    /// <c>Internal*Async</c> walker; cooperative checks at the Phase 3→4 and Phase 4→5 boundaries
    /// close the windows where cancellation observed late in a phase would otherwise reach the next
    /// phase's work uncancelled.
    /// </para>
    /// <para>
    /// Disposal contract on non-normal exit: on any thrown exception the catch handler disposes
    /// the already-built providers in reverse order (final before transient), preferring
    /// <see cref="IAsyncDisposable.DisposeAsync"/> and falling back to <see cref="IDisposable.Dispose"/>.
    /// If either dispose call throws, the original exception is preserved as
    /// <c>InnerExceptions[0]</c> of an <see cref="AggregateException"/> whose subsequent inner
    /// exceptions are the dispose errors in the order they occurred.
    /// </para>
    /// <para>
    /// The builder is single-use on success only: a second call after a successful build throws
    /// <see cref="InvalidOperationException"/>. A failed build leaves <c>_isAlreadyBuilt</c> false
    /// so the caller may retry after addressing the fault.
    /// </para>
    /// <para>
    /// Pack authors: services materialized via the transient provider passed to <c>RegisterAsync</c>
    /// are released when the transient provider is disposed (immediately before <c>SetupAsync</c> runs).
    /// Do not cache references obtained from the transient provider beyond <c>RegisterAsync</c>;
    /// resolve again from the final provider in <c>SetupAsync</c> if needed.
    /// </para>
    /// <para>
    /// Pack authors that spawn fire-and-forget work from <c>SetupAsync</c> without observing
    /// <paramref name="ct"/> may leak after a downstream cancellation — same risk as today's sync
    /// model; honour the token to participate in cooperative shutdown.
    /// </para>
    /// </summary>
    /// <param name="ct">Cancellation token threaded to every pack phase</param>
    /// <returns>The built service provider.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the builder has already produced a provider successfully.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="ct"/> is cancelled during a pack await or at a phase boundary check.</exception>
    /// <exception cref="AggregateException">Thrown when a dispose call inside the catch handler throws; <c>InnerExceptions[0]</c> is the original phase/cancel exception.</exception>
    public async Task<ServiceProvider> BuildAsync(CancellationToken ct)
    {
        if (_isAlreadyBuilt)
            throw new InvalidOperationException("ServiceProviderBuilder is already built");

        ServiceProvider? transient = null;
        ServiceProvider? final = null;

        // Work on a clone of _container so a Configure/Register failure leaves _container
        // unchanged — the caller can retry from a clean baseline.
        var workingContainer = _container.Clone();

        try
        {
            // Phase 1: Configure — accumulate in a staging container, then merge into the working clone
            var configurationContainer = new ServiceContainer();
            foreach (var pack in _packs)
                await pack.InternalConfigureAsync(configurationContainer, ct).ConfigureAwait(false);

            foreach (var descriptor in configurationContainer)
                workingContainer.Add(descriptor);

            // Phase 2: build transient provider for RegisterAsync's provider parameter
            transient = workingContainer.BuildServiceProvider();

            // Phase 3: RegisterAsync — packs may consume Configure-phase services via transient
            // while adding additional registrations to workingContainer
            foreach (var pack in _packs)
                await pack.InternalRegisterAsync(workingContainer, transient, ct).ConfigureAwait(false);

            // Boundary check Phase 3 → Phase 4: cancellation observed between the last Phase 3 await
            // and Phase 4's sync work surfaces here.
            ct.ThrowIfCancellationRequested();

            // Phase 4: build the final provider, then dispose the transient (async-first) and
            // null the local so the catch handler does not double-dispose if a later phase faults.
            final = workingContainer.BuildServiceProvider();
            await transient.DisposeAsync().ConfigureAwait(false);
            transient = null;

            // Boundary check Phase 4 → Phase 5: cancellation observed during Phase 4's sync work
            // surfaces here, before any SetupAsync runs.
            ct.ThrowIfCancellationRequested();

            // Phase 5: SetupAsync on the final provider
            foreach (var pack in _packs)
                await pack.InternalSetupAsync(final, ct).ConfigureAwait(false);
        }
        catch (Exception original)
        {
            await DisposeWithAggregationAsync(original, final, transient).ConfigureAwait(false);
            throw;
        }

        _isAlreadyBuilt = true;

        return final;
    }

    /// <summary>
    /// Reverse-order disposal of partial build state. Iterates [final, transient] (nulls skipped);
    /// each provider is disposed via <see cref="IAsyncDisposable.DisposeAsync"/> if implemented,
    /// otherwise <see cref="IDisposable.Dispose"/>. If any dispose throws, the original phase/cancel
    /// exception is rethrown as <c>InnerExceptions[0]</c> of an <see cref="AggregateException"/>;
    /// dispose errors follow in the order they occurred. If all disposes succeed, the method returns
    /// normally and the caller's <c>throw;</c> rethrows the original with stack preserved.
    /// </summary>
    /// <param name="original">The original phase or cancellation exception that triggered teardown; preserved as the head of any aggregated failure.</param>
    /// <param name="final">The final provider built so far, if any; disposed first. May be null.</param>
    /// <param name="transient">The transient provider built during the failing phase, if any; disposed after <paramref name="final"/>. May be null.</param>
    /// <returns>A task that completes once disposal finishes; it throws rather than returning a value when a dispose fails.</returns>
    internal static async Task DisposeWithAggregationAsync(
        Exception original,
        ServiceProvider? final,
        ServiceProvider? transient
    )
    {
        List<Exception>? disposeErrors = null;
        foreach (var sp in new[] { final, transient })
        {
            if (sp is null)
                continue;

            try
            {
                await sp.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                (disposeErrors ??= new List<Exception>()).Add(e);
            }
        }

        if (disposeErrors is not null)
            throw new AggregateException(new[] { original }.Concat(disposeErrors));
    }
}
