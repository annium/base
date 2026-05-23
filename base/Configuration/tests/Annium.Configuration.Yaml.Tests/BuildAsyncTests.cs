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
    /// Pointing <c>AddYamlFile(optional: false)</c> at a missing file makes <c>BuildAsync</c>
    /// throw <see cref="AggregateException"/> wrapping a <see cref="FileNotFoundException"/>.
    /// </summary>
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
    /// Mirror of the Json test: a remote source returning a non-2xx response surfaces an
    /// <see cref="HttpRequestException"/> wrapped in <see cref="AggregateException"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_Non2xxResponse_ThrowsHttpRequestException()
    {
        await using var stub = new StaticResponseTcpListener(
            HttpStatusCode.InternalServerError,
            "value: ok",
            "config.yaml",
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
    [Fact]
    public async Task LoadAsync_CtCancelled_ThrowsOperationCanceledException()
    {
        await using var stub = new HangingTcpListener("config.yaml");
        await stub.StartAsync(TestContext.Current.CancellationToken);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteYaml(stub.Uri, optional: false, timeout: TimeSpan.FromSeconds(30));

        var ex = await Wrap.It(async () => await container.BuildAsync(cts.Token)).ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        var inner = ex.InnerExceptions[0];
        var isCancel = inner is OperationCanceledException;
        isCancel.IsTrue($"expected OperationCanceledException; got {inner.GetType().FullName}: {inner.Message}");
    }

    /// <summary>
    /// Mirror of the Json timeout test: a hanging remote endpoint with <c>optional: false</c>
    /// surfaces a <see cref="TimeoutException"/> or <see cref="HttpRequestException"/> wrapped in
    /// <see cref="AggregateException"/>.
    /// </summary>
    [Fact]
    public async Task BuildAsync_AddRemoteYaml_TimeoutNotOptional_Throws()
    {
        await using var stub = new HangingTcpListener("config.yaml");
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
    [Fact]
    public async Task BuildAsync_AddRemoteYaml_TimeoutOptional_Succeeds()
    {
        await using var stub = new HangingTcpListener("config.yaml");
        await stub.StartAsync(TestContext.Current.CancellationToken);

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteYaml(stub.Uri, optional: true, timeout: TimeSpan.FromMilliseconds(500));

        await container.BuildAsync(TestContext.Current.CancellationToken);

        container.Get().Count.Is(0);
    }
}
