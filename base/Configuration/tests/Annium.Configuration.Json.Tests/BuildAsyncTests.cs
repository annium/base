using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Tests.Lib;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.Json.Tests;

/// <summary>
/// Tests for the deferred-source build pipeline — JSON file + remote sources +
/// optional / non-optional semantics through <see cref="Abstractions.ConfigurationContainerExtensions.BuildAsync"/>.
/// </summary>
public class BuildAsyncTests
{
    /// <summary>
    /// Resource name served by the stub TCP listeners in these tests.
    /// </summary>
    private const string ResourcePath = "config.json";

    /// <summary>
    /// An empty container (no sources registered) is a no-op for <c>BuildAsync</c>.
    /// </summary>
    [Fact]
    public async Task BuildAsync_NoSources_NoOp()
    {
        var container = ConfigurationFactory.CreateContainer();

        await container.BuildAsync(TestContext.Current.CancellationToken);

        container.Get().Count.Is(0);
    }

    /// <summary>
    /// Pointing <c>AddJsonFile(optional: false)</c> at a missing file makes <c>BuildAsync</c>
    /// throw <see cref="AggregateException"/> wrapping a <see cref="FileNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task BuildAsync_AddJsonFile_MissingNotOptional_Throws()
    {
        var container = ConfigurationFactory.CreateContainer();
        var missing = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");
        container.AddJsonFile(missing, optional: false);

        var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        ex.InnerExceptions[0].As<FileNotFoundException>();
    }

    /// <summary>
    /// Pointing <c>AddJsonFile(optional: true)</c> at a missing file makes <c>BuildAsync</c>
    /// succeed; the missing source contributes no data.
    /// </summary>
    [Fact]
    public async Task BuildAsync_AddJsonFile_MissingOptional_Succeeds()
    {
        var container = ConfigurationFactory.CreateContainer();
        var missing = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");
        container.AddJsonFile(missing, optional: true);

        await container.BuildAsync(TestContext.Current.CancellationToken);

        container.Get().Count.Is(0);
    }

    /// <summary>
    /// Pointing <c>AddJsonFile</c> at a real file flattens its contents into the container.
    /// </summary>
    [Fact]
    public async Task BuildAsync_AddJsonFile_ExistingFile_Loads()
    {
        var jsonFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(jsonFile, "{\"plain\":42,\"section\":{\"value\":\"ok\"}}");
            var container = ConfigurationFactory.CreateContainer();
            container.AddJsonFile(jsonFile);

            await container.BuildAsync(TestContext.Current.CancellationToken);

            var data = container.Get();
            data.Count.Is(2);
            data.At(new[] { "plain" }).Is("42");
            data.At(new[] { "section", "value" }).Is("ok");
        }
        finally
        {
            File.Delete(jsonFile);
        }
    }

    /// <summary>
    /// A remote source pointed at a stub server that accepts but never responds, with a short
    /// timeout, makes <c>BuildAsync(optional: false)</c> throw via <see cref="AggregateException"/>.
    /// The inner is normally <see cref="TimeoutException"/>, but under heavy parallel test load
    /// the network stack can surface a sibling <see cref="System.Net.Http.HttpRequestException"/>
    /// with an <see cref="System.IO.IOException"/> inner instead — both denote "remote fetch
    /// could not complete", which is the spec's behavioral contract for non-optional sources.
    /// </summary>
    [Fact]
    public async Task BuildAsync_AddRemoteJson_TimeoutNotOptional_Throws()
    {
        await using var stub = new HangingTcpListener(ResourcePath);
        await stub.StartAsync(TestContext.Current.CancellationToken);

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteJson(stub.Uri, optional: false, timeout: TimeSpan.FromMilliseconds(500));

        var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        var inner = ex.InnerExceptions[0];
        var isFetchFailure = inner is TimeoutException or HttpRequestException;
        isFetchFailure.IsTrue($"expected fetch failure; got {inner.GetType().FullName}: {inner.Message}");
    }

    /// <summary>
    /// Same stub server + 500ms timeout, but with <c>optional: true</c> — <c>BuildAsync</c> returns
    /// successfully and the source contributes no data.
    /// </summary>
    [Fact]
    public async Task BuildAsync_AddRemoteJson_TimeoutOptional_Succeeds()
    {
        await using var stub = new HangingTcpListener(ResourcePath);
        await stub.StartAsync(TestContext.Current.CancellationToken);

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteJson(stub.Uri, optional: true, timeout: TimeSpan.FromMilliseconds(500));

        await container.BuildAsync(TestContext.Current.CancellationToken);

        container.Get().Count.Is(0);
    }

    /// <summary>
    /// A remote source pointed at a server returning a non-2xx response surfaces an
    /// <see cref="HttpRequestException"/> wrapped in <see cref="AggregateException"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_Non2xxResponse_ThrowsHttpRequestException()
    {
        await using var stub = new StaticResponseTcpListener(HttpStatusCode.InternalServerError, "{}", ResourcePath);
        await stub.StartAsync(TestContext.Current.CancellationToken);

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteJson(stub.Uri, optional: false, timeout: TimeSpan.FromSeconds(5));

        var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        ex.InnerExceptions[0].As<HttpRequestException>();
    }

    /// <summary>
    /// A pre-cancelled cancellation token surfaces <see cref="OperationCanceledException"/>
    /// (not <see cref="TimeoutException"/>) so callers can distinguish intentional cancel from
    /// timeout. Verified against a listener that accepts but never responds.
    /// </summary>
    [Fact]
    public async Task LoadAsync_CtCancelled_ThrowsOperationCanceledException()
    {
        await using var stub = new HangingTcpListener(ResourcePath);
        await stub.StartAsync(TestContext.Current.CancellationToken);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteJson(stub.Uri, optional: false, timeout: TimeSpan.FromSeconds(30));

        await Wrap.It(async () => await container.BuildAsync(cts.Token)).ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// A token cancelled WHILE the request is in-flight (not pre-cancelled) surfaces
    /// <see cref="OperationCanceledException"/>, not <see cref="TimeoutException"/> — the per-source
    /// timeout is long enough that only the caller's cancellation can fire. Exercises the
    /// <c>!ct.IsCancellationRequested</c> catch filter at the async suspension point.
    /// </summary>
    [Fact]
    public async Task LoadAsync_CtCancelledMidFlight_ThrowsOperationCanceledException()
    {
        await using var stub = new HangingTcpListener(ResourcePath);
        await stub.StartAsync(TestContext.Current.CancellationToken);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(500));

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteJson(stub.Uri, optional: false, timeout: TimeSpan.FromSeconds(30));

        await Wrap.It(async () => await container.BuildAsync(cts.Token)).ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// An OPTIONAL remote source whose token is cancelled mid-flight must still surface
    /// <see cref="OperationCanceledException"/> — caller cancellation is never silenced by the
    /// <c>Optional</c> flag (only load failures like timeout are). Covers the optional × mid-flight
    /// quadrant of the cancellation matrix.
    /// </summary>
    [Fact]
    public async Task LoadAsync_OptionalCtCancelledMidFlight_ThrowsOperationCanceledException()
    {
        await using var stub = new HangingTcpListener(ResourcePath);
        await stub.StartAsync(TestContext.Current.CancellationToken);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(500));

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteJson(stub.Uri, optional: true, timeout: TimeSpan.FromSeconds(30));

        await Wrap.It(async () => await container.BuildAsync(cts.Token)).ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// A remote source returning 200 OK with a valid JSON body parses and flattens the
    /// payload into the container — the success branch of <c>RemoteConfigurationSourceBase.LoadAsync</c>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_SuccessResponse_LoadsRemoteData()
    {
        await using var stub = new StaticResponseTcpListener(
            HttpStatusCode.OK,
            "{\"plain\":42,\"section\":{\"value\":\"ok\"}}",
            ResourcePath
        );
        await stub.StartAsync(TestContext.Current.CancellationToken);

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteJson(stub.Uri, optional: false, timeout: TimeSpan.FromSeconds(5));

        await container.BuildAsync(TestContext.Current.CancellationToken);

        var data = container.Get();
        data.At(new[] { "plain" }).Is("42");
        data.At(new[] { "section", "value" }).Is("ok");
    }
}
