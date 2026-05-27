using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.Abstractions.Tests;

/// <summary>
/// Verifies <see cref="ConfigurationContainerExtensions.BuildAsync"/> registration-order merge
/// semantics and aggregate-failure handling, independent of any specific source package.
/// </summary>
public class BuildAsyncMergeOrderTests
{
    /// <summary>
    /// When two sources contribute overlapping keys, the source registered later wins.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_TwoSources_MergesInRegistrationOrder()
    {
        var container = ConfigurationFactory.CreateContainer();
        container.AddSource(new StubSource(new[] { (new[] { "key" }, "first") }, optional: false));
        container.AddSource(new StubSource(new[] { (new[] { "key" }, "second") }, optional: false));

        await container.BuildAsync(TestContext.Current.CancellationToken);

        var data = container.Get();
        data.At(new[] { "key" }).Is("second");
    }

    /// <summary>
    /// A non-optional source that throws makes <c>BuildAsync</c> surface the failure as an
    /// <see cref="AggregateException"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_OneNonOptionalFails_ThrowsAggregate()
    {
        var container = ConfigurationFactory.CreateContainer();
        container.AddSource(new StubSource(new[] { (new[] { "ok" }, "v") }, optional: false));
        container.AddSource(new ThrowingSource(new InvalidOperationException("source down"), optional: false));

        var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        ex.InnerExceptions[0].As<InvalidOperationException>();
    }

    /// <summary>
    /// Two non-optional sources that throw distinct exception types aggregate both into the
    /// resulting <see cref="AggregateException"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_TwoNonOptionalFail_AggregateContainsBothErrors()
    {
        var container = ConfigurationFactory.CreateContainer();
        container.AddSource(new ThrowingSource(new InvalidOperationException("first down"), optional: false));
        container.AddSource(new ThrowingSource(new NotSupportedException("second down"), optional: false));

        var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(2);
        // Order matches Task.WhenAll input order = registration order.
        ex.InnerExceptions[0].As<InvalidOperationException>();
        ex.InnerExceptions[1].As<NotSupportedException>();
    }

    /// <summary>
    /// An optional source that throws is silenced; the surviving source's data lands in the container.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_OneOptionalFails_OneSucceeds_SucceedsWithSucceededData()
    {
        var container = ConfigurationFactory.CreateContainer();
        container.AddSource(new ThrowingSource(new InvalidOperationException("flaky"), optional: true));
        container.AddSource(new StubSource(new[] { (new[] { "ok" }, "value") }, optional: false));

        await container.BuildAsync(TestContext.Current.CancellationToken);

        var data = container.Get();
        data.Count.Is(1);
        data.At(new[] { "ok" }).Is("value");
    }

    /// <summary>
    /// When every source is optional and every one throws, <c>BuildAsync</c> raises no exception
    /// (the non-optional failure filter yields nothing) and the container is left empty.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_AllSourcesOptionalAndAllFail_SucceedsWithEmptyContainer()
    {
        var container = ConfigurationFactory.CreateContainer();
        container.AddSource(new ThrowingSource(new InvalidOperationException("first down"), optional: true));
        container.AddSource(new ThrowingSource(new NotSupportedException("second down"), optional: true));

        await container.BuildAsync(TestContext.Current.CancellationToken);

        container.Get().Count.Is(0);
    }

    /// <summary>
    /// An optional source that observes a pre-cancelled token must NOT swallow the caller's
    /// cancellation: <c>BuildAsync</c> propagates <see cref="OperationCanceledException"/> directly,
    /// regardless of the source's <c>Optional</c> flag. Locks in the optional-source branch of the
    /// <c>catch (OperationCanceledException) when (ct.IsCancellationRequested)</c> guard.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_OptionalSourcePrecancelledCt_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var container = ConfigurationFactory.CreateContainer();
        container.AddSource(new CancelObservingSource(optional: true));

        await Wrap.It(async () => await container.BuildAsync(cts.Token)).ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// In-memory source returning a fixed dictionary on <c>LoadAsync</c>.
    /// </summary>
    private sealed class StubSource : IConfigurationSource
    {
        /// <summary>The fixed key-value data returned by <c>LoadAsync</c>.</summary>
        private readonly IReadOnlyDictionary<string[], string> _data;

        public StubSource(IEnumerable<(string[] key, string value)> entries, bool optional)
        {
            var dict = new Dictionary<string[], string>();
            foreach (var (key, value) in entries)
                dict[key] = value;
            _data = dict;
            Optional = optional;
        }

        /// <summary>Gets a value indicating whether load failures are silently ignored.</summary>
        public bool Optional { get; }

        /// <summary>Returns the fixed in-memory data immediately.</summary>
        /// <param name="ct">Cancellation token (unused — the result is already in memory).</param>
        /// <returns>A completed value task carrying the fixed key-value dictionary.</returns>
        public ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct) => new(_data);
    }

    /// <summary>
    /// Source that throws on <c>LoadAsync</c> — used to verify aggregate / optional handling.
    /// </summary>
    private sealed class ThrowingSource : IConfigurationSource
    {
        /// <summary>The exception thrown unconditionally by <c>LoadAsync</c>.</summary>
        private readonly Exception _ex;

        public ThrowingSource(Exception ex, bool optional)
        {
            _ex = ex;
            Optional = optional;
        }

        /// <summary>Gets a value indicating whether load failures are silently ignored.</summary>
        public bool Optional { get; }

        /// <summary>Always throws the pre-configured exception to simulate a failing source.</summary>
        /// <param name="ct">Cancellation token (unused — the exception is thrown unconditionally).</param>
        /// <returns>Never returns; always throws.</returns>
        public ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct) => throw _ex;
    }

    /// <summary>
    /// Source that honors the cancellation token on <c>LoadAsync</c> — used to verify that
    /// caller cancellation propagates even when the source is optional.
    /// </summary>
    private sealed class CancelObservingSource : IConfigurationSource
    {
        /// <summary>Shared empty dictionary returned when the cancellation token is not signalled.</summary>
        private static readonly IReadOnlyDictionary<string[], string> _empty = new Dictionary<string[], string>();

        public CancelObservingSource(bool optional)
        {
            Optional = optional;
        }

        /// <summary>Gets a value indicating whether load failures are silently ignored.</summary>
        public bool Optional { get; }

        /// <summary>
        /// Throws <see cref="OperationCanceledException"/> if <paramref name="ct"/> is already
        /// cancelled; otherwise returns an empty dictionary.
        /// </summary>
        /// <param name="ct">Cancellation token observed before returning data.</param>
        /// <returns>A completed value task carrying an empty key-value dictionary, or throws if cancelled.</returns>
        public ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return new(_empty);
        }
    }
}
