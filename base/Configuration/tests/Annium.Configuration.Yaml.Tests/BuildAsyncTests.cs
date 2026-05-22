using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
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
        using var stub = new StaticResponseTcpListener(HttpStatusCode.InternalServerError, "value: ok");
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
        using var stub = new HangingTcpListener();
        await stub.StartAsync(TestContext.Current.CancellationToken);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var container = ConfigurationFactory.CreateContainer();
        container.AddRemoteYaml(stub.Uri, optional: false, timeout: TimeSpan.FromSeconds(30));

        var ex = await Wrap.It(async () => await container.BuildAsync(cts.Token))
            .ThrowsAsync<AggregateException>();
        ex.InnerExceptions.Has(1);
        var inner = ex.InnerExceptions[0];
        var isCancel = inner is OperationCanceledException;
        isCancel.IsTrue($"expected OperationCanceledException; got {inner.GetType().FullName}: {inner.Message}");
    }

    /// <summary>
    /// Local TCP listener that accepts connections but never sends a response. Mirrors the Json
    /// test helper to keep the YAML coverage symmetric.
    /// </summary>
    private sealed class HangingTcpListener : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly List<TcpClient> _accepted = new();
        private readonly TaskCompletionSource _listening = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public HangingTcpListener()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
        }

        public Uri Uri => new($"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/config.yaml");

        public async Task StartAsync(CancellationToken ct)
        {
            _listener.Start();
            _ = Task.Run(async () =>
            {
                _listening.TrySetResult();
                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                        lock (_accepted)
                            _accepted.Add(client);
                    }
                }
                catch (OperationCanceledException) { }
                catch (ObjectDisposedException) { }
            });

            await _listening.Task.WaitAsync(ct);
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

    /// <summary>
    /// Local TCP listener that returns a fixed HTTP status code + body for any incoming request.
    /// </summary>
    private sealed class StaticResponseTcpListener : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly HttpStatusCode _status;
        private readonly string _body;
        private readonly TaskCompletionSource _listening = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public StaticResponseTcpListener(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
            _listener = new TcpListener(IPAddress.Loopback, 0);
        }

        public Uri Uri => new($"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}/config.yaml");

        public async Task StartAsync(CancellationToken ct)
        {
            _listener.Start();
            _ = Task.Run(async () =>
            {
                _listening.TrySetResult();
                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        using var client = await _listener.AcceptTcpClientAsync(_cts.Token);
                        await using var stream = client.GetStream();

                        var buffer = new byte[4096];
                        try
                        {
                            using var readCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
                            _ = await stream.ReadAsync(buffer, readCts.Token);
                        }
                        catch (OperationCanceledException) { }

                        var bodyBytes = Encoding.UTF8.GetBytes(_body);
                        var reasonPhrase = _status.ToString();
                        var header = Encoding.ASCII.GetBytes(
                            $"HTTP/1.1 {(int)_status} {reasonPhrase}\r\n"
                                + $"Content-Length: {bodyBytes.Length}\r\n"
                                + "Content-Type: application/x-yaml\r\n"
                                + "Connection: close\r\n"
                                + "\r\n"
                        );
                        await stream.WriteAsync(header, _cts.Token);
                        if (bodyBytes.Length > 0)
                            await stream.WriteAsync(bodyBytes, _cts.Token);
                        await stream.FlushAsync(_cts.Token);
                    }
                }
                catch (OperationCanceledException) { }
                catch (ObjectDisposedException) { }
                catch (IOException) { }
            });

            await _listening.Task.WaitAsync(ct);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _listener.Stop();
            _cts.Dispose();
        }
    }
}
