using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Abstractions.Internal;
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
    [Fact]
    public async Task BuildAsync_TwoSources_MergesInRegistrationOrder()
    {
        var container = new ConfigurationContainer();
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
    [Fact]
    public async Task BuildAsync_OneNonOptionalFails_ThrowsAggregate()
    {
        var container = new ConfigurationContainer();
        container.AddSource(new StubSource(new[] { (new[] { "ok" }, "v") }, optional: false));
        container.AddSource(new ThrowingSource(new InvalidOperationException("source down"), optional: false));

        var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        ex.InnerExceptions[0].As<InvalidOperationException>();
    }

    /// <summary>
    /// An optional source that throws is silenced; the surviving source's data lands in the container.
    /// </summary>
    [Fact]
    public async Task BuildAsync_OneOptionalFails_OneSucceeds_SucceedsWithSucceededData()
    {
        var container = new ConfigurationContainer();
        container.AddSource(new ThrowingSource(new InvalidOperationException("flaky"), optional: true));
        container.AddSource(new StubSource(new[] { (new[] { "ok" }, "value") }, optional: false));

        await container.BuildAsync(TestContext.Current.CancellationToken);

        var data = container.Get();
        data.Count.Is(1);
        data.At(new[] { "ok" }).Is("value");
    }

    /// <summary>
    /// In-memory source returning a fixed dictionary on <c>LoadAsync</c>.
    /// </summary>
    private sealed class StubSource : IConfigurationSource
    {
        private readonly IReadOnlyDictionary<string[], string> _data;

        public StubSource(IEnumerable<(string[] key, string value)> entries, bool optional)
        {
            var dict = new Dictionary<string[], string>();
            foreach (var (key, value) in entries)
                dict[key] = value;
            _data = dict;
            Optional = optional;
        }

        public bool Optional { get; }

        public ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct) => new(_data);
    }

    /// <summary>
    /// Source that throws on <c>LoadAsync</c> — used to verify aggregate / optional handling.
    /// </summary>
    private sealed class ThrowingSource : IConfigurationSource
    {
        private readonly Exception _ex;

        public ThrowingSource(Exception ex, bool optional)
        {
            _ex = ex;
            Optional = optional;
        }

        public bool Optional { get; }

        public ValueTask<IReadOnlyDictionary<string[], string>> LoadAsync(CancellationToken ct) => throw _ex;
    }
}
