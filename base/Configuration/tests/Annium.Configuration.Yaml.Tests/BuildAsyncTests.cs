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

namespace Annium.Configuration.Yaml.Tests;

/// <summary>
/// Tests for the deferred-source build pipeline — YAML file + remote sources +
/// optional / non-optional semantics through <see cref="Abstractions.ConfigurationContainerExtensions.BuildAsync"/>.
/// </summary>
public class BuildAsyncTests
{
    /// <summary>
    /// Resource name served by the stub TCP listeners in these tests.
    /// </summary>
    private const string ResourcePath = "config.yaml";

    /// <summary>
    /// Pointing <c>AddYamlFile(optional: false)</c> at a missing file makes <c>BuildAsync</c>
    /// throw <see cref="AggregateException"/> wrapping a <see cref="FileNotFoundException"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_AddYamlFile_MissingNotOptional_Throws()
    {
        var container = ConfigurationFactory.CreateContainer();
        var missing = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.yaml");
        container.AddYamlFile(missing, optional: false);

        var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        ex.InnerExceptions[0].As<FileNotFoundException>();
    }

    /// <summary>
    /// Pointing <c>AddYamlFile(optional: true)</c> at a missing file makes <c>BuildAsync</c>
    /// succeed; the missing source contributes no data.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_AddYamlFile_MissingOptional_Succeeds()
    {
        var container = ConfigurationFactory.CreateContainer();
        var missing = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.yaml");
        container.AddYamlFile(missing, optional: true);

        await container.BuildAsync(TestContext.Current.CancellationToken);

        container.Get().Count.Is(0);
    }

    /// <summary>
    /// Pointing <c>AddYamlFile</c> at a real file flattens its contents into the container
    /// (mirror of the Json existing-file test — exercises the YamlFileSource happy path through
    /// the BuildAsync pipeline, not just the provider).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_AddYamlFile_ExistingFile_Loads()
    {
        var yamlFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(yamlFile, "plain: 42\nsection:\n  value: ok\n");
            var container = ConfigurationFactory.CreateContainer();
            container.AddYamlFile(yamlFile);

            await container.BuildAsync(TestContext.Current.CancellationToken);

            var data = container.Get();
            data.Count.Is(2);
            data.At(new[] { "plain" }).Is("42");
            data.At(new[] { "section", "value" }).Is("ok");
        }
        finally
        {
            File.Delete(yamlFile);
        }
    }

    /// <summary>
    /// Mirror of the Json test: a remote source returning a non-2xx response surfaces an
    /// <see cref="HttpRequestException"/> wrapped in <see cref="AggregateException"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task LoadAsync_Non2xxResponse_ThrowsHttpRequestException()
    {
        await using var stub = new StaticResponseTcpListener(
            HttpStatusCode.InternalServerError,
            "value: ok",
            ResourcePath,
            contentType: "application/x-yaml"
        );
        await stub.StartAsync(TestContext.Current.CancellationToken);

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteYaml(stub.Uri, optional: false, timeout: TimeSpan.FromSeconds(5));

        var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        ex.InnerExceptions[0].As<HttpRequestException>();
    }

    /// <summary>
    /// Mirror of the Json test: a pre-cancelled CT surfaces <see cref="OperationCanceledException"/>
    /// (not <see cref="TimeoutException"/>).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task LoadAsync_CtCancelled_ThrowsOperationCanceledException()
    {
        await using var stub = new HangingTcpListener(ResourcePath);
        await stub.StartAsync(TestContext.Current.CancellationToken);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteYaml(stub.Uri, optional: false, timeout: TimeSpan.FromSeconds(30));

        await Wrap.It(async () => await container.BuildAsync(cts.Token)).ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Mirror of the Json mid-flight test: a token cancelled WHILE the request is in-flight
    /// surfaces <see cref="OperationCanceledException"/>, not <see cref="TimeoutException"/> — the
    /// per-source timeout is long enough that only the caller's cancellation can fire. Exercises
    /// the <c>!ct.IsCancellationRequested</c> catch filter at the async suspension point.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task LoadAsync_CtCancelledMidFlight_ThrowsOperationCanceledException()
    {
        await using var stub = new HangingTcpListener(ResourcePath);
        await stub.StartAsync(TestContext.Current.CancellationToken);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(500));

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteYaml(stub.Uri, optional: false, timeout: TimeSpan.FromSeconds(30));

        await Wrap.It(async () => await container.BuildAsync(cts.Token)).ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// An OPTIONAL remote source whose token is cancelled mid-flight must still surface
    /// <see cref="OperationCanceledException"/> — caller cancellation is never silenced by the
    /// <c>Optional</c> flag (only load failures like timeout are). Covers the optional × mid-flight
    /// quadrant of the cancellation matrix.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task LoadAsync_OptionalCtCancelledMidFlight_ThrowsOperationCanceledException()
    {
        await using var stub = new HangingTcpListener(ResourcePath);
        await stub.StartAsync(TestContext.Current.CancellationToken);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(500));

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteYaml(stub.Uri, optional: true, timeout: TimeSpan.FromSeconds(30));

        await Wrap.It(async () => await container.BuildAsync(cts.Token)).ThrowsAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Mirror of the Json timeout test: a hanging remote endpoint with <c>optional: false</c>
    /// surfaces a <see cref="TimeoutException"/> or <see cref="HttpRequestException"/> wrapped in
    /// <see cref="AggregateException"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_AddRemoteYaml_TimeoutNotOptional_Throws()
    {
        await using var stub = new HangingTcpListener(ResourcePath);
        await stub.StartAsync(TestContext.Current.CancellationToken);

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteYaml(stub.Uri, optional: false, timeout: TimeSpan.FromMilliseconds(500));

        var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        var inner = ex.InnerExceptions[0];
        var isFetchFailure = inner is TimeoutException or HttpRequestException;
        isFetchFailure.IsTrue($"expected fetch failure; got {inner.GetType().FullName}: {inner.Message}");
    }

    /// <summary>
    /// Mirror of the Json timeout test: same hanging stub with <c>optional: true</c> succeeds
    /// and the source contributes no data.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task BuildAsync_AddRemoteYaml_TimeoutOptional_Succeeds()
    {
        await using var stub = new HangingTcpListener(ResourcePath);
        await stub.StartAsync(TestContext.Current.CancellationToken);

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteYaml(stub.Uri, optional: true, timeout: TimeSpan.FromMilliseconds(500));

        await container.BuildAsync(TestContext.Current.CancellationToken);

        container.Get().Count.Is(0);
    }

    /// <summary>
    /// A remote source returning 200 OK with a valid YAML body parses and flattens the
    /// payload into the container — the success branch of <c>RemoteConfigurationSourceBase.LoadAsync</c>
    /// for the YAML flavour (mirror of the Json <c>LoadAsync_SuccessResponse_LoadsRemoteData</c> test).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task LoadAsync_SuccessResponse_LoadsRemoteData()
    {
        await using var stub = new StaticResponseTcpListener(
            HttpStatusCode.OK,
            "plain: 42\nsection:\n  value: ok\n",
            ResourcePath,
            contentType: "application/x-yaml"
        );
        await stub.StartAsync(TestContext.Current.CancellationToken);

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteYaml(stub.Uri, optional: false, timeout: TimeSpan.FromSeconds(5));

        await container.BuildAsync(TestContext.Current.CancellationToken);

        var data = container.Get();
        data.At(new[] { "plain" }).Is("42");
        data.At(new[] { "section", "value" }).Is("ok");
    }
}
