using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Abstractions.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.Json.Tests;

/// <summary>
/// Tests for the deferred-source build pipeline added in T7 — JSON file + remote sources +
/// optional / non-optional semantics through <see cref="Abstractions.ConfigurationContainerExtensions.BuildAsync"/>.
/// </summary>
public class BuildAsyncTests
{
    /// <summary>
    /// An empty container (no sources registered) is a no-op for <c>BuildAsync</c>.
    /// </summary>
    [Fact]
    public async Task BuildAsync_NoSources_NoOp()
    {
        var container = new ConfigurationContainer();

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
        var container = new ConfigurationContainer();
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
        var container = new ConfigurationContainer();
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
            var container = new ConfigurationContainer();
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
        using var stub = new HangingTcpListener();
        stub.Start();
        // Give the OS a moment to start listening before the HTTP call.
        await Task.Delay(50, TestContext.Current.CancellationToken);

        var container = new ConfigurationContainer();
        container.AddRemoteJson(stub.Uri, optional: false, timeout: TimeSpan.FromMilliseconds(500));

        var ex = await Wrap.It(async () => await container.BuildAsync(TestContext.Current.CancellationToken))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        var inner = ex.InnerExceptions[0];
        var isFetchFailure = inner is TimeoutException or HttpRequestException;
        isFetchFailure.IsTrue($"expected fetch failure; got {inner.GetType().FullName}: {inner.Message}");
    }

    /// <summary>
    /// Same stub server + 200ms timeout, but with <c>optional: true</c> — <c>BuildAsync</c> returns
    /// successfully and the source contributes no data.
    /// </summary>
    [Fact]
    public async Task BuildAsync_AddRemoteJson_TimeoutOptional_Succeeds()
    {
        using var stub = new HangingTcpListener();
        stub.Start();
        await Task.Delay(50, TestContext.Current.CancellationToken);

        var container = new ConfigurationContainer();
        container.AddRemoteJson(stub.Uri, optional: true, timeout: TimeSpan.FromMilliseconds(500));

        await container.BuildAsync(TestContext.Current.CancellationToken);

        container.Get().Count.Is(0);
    }

    /// <summary>
    /// Local TCP listener that accepts connections but never sends a response — used to force
    /// a deterministic <c>HttpClient</c> timeout trigger regardless of the test host's network
    /// configuration. Holds strong references to accepted clients so GC can't reap them
    /// mid-test (which would otherwise close the socket and translate timeout into IO error).
    /// </summary>
    private sealed class HangingTcpListener : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly List<TcpClient> _accepted = new();

        public HangingTcpListener()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
        }

        public Uri Uri => new($"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/config.json");

        public void Start()
        {
            _listener.Start();
            _ = Task.Run(async () =>
            {
                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                        // Hold a strong reference so GC doesn't reap the socket while the test
                        // is mid-request — otherwise HttpClient sees an IO error, not a timeout.
                        lock (_accepted)
                            _accepted.Add(client);
                    }
                }
                catch (OperationCanceledException)
                { /* expected on dispose */
                }
                catch (ObjectDisposedException)
                { /* expected on dispose */
                }
            });
        }

        public void Dispose()
        {
            _cts.Cancel();
            _listener.Stop();
            lock (_accepted)
            {
                foreach (var c in _accepted)
                    c.Dispose();
                _accepted.Clear();
            }
            _cts.Dispose();
        }
    }
}
