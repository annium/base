using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Sockets.Tests;

public class ClientServerSocketSslTests : ClientServerSocketTestsBase
{
    public ClientServerSocketSslTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Fact]
    public async Task Send_NotConnected()
    {
        this.Trace("start");

        await Send_NotConnected_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Canceled()
    {
        this.Trace("start");

        await Send_Canceled_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Send_ClientClosed()
    {
        this.Trace("start");

        await Send_ClientClosed_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Send_ServerClosed()
    {
        this.Trace("start");

        await Send_ServerClosed_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Normal()
    {
        this.Trace("start");

        await Send_Normal_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Reconnect()
    {
        this.Trace("start");

        await Send_Reconnect_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_Normal()
    {
        this.Trace("start");

        await Listen_Normal_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_SmallBuffer()
    {
        this.Trace("start");

        await Listen_SmallBuffer_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_Reconnect()
    {
        this.Trace("start");

        await Listen_Reconnect_Base();

        this.Trace("done");
    }

    protected override void HandleConnect(IClientSocket socket)
    {
        this.Trace("start");

        var authOptions = new SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = (_, _, _, _) => true,
        };

        socket.Connect(EndPoint, authOptions);

        this.Trace("done");
    }

    protected override IAsyncDisposable RunServer(Func<IServerSocket, CancellationToken, Task> handleSocket)
    {
        var cert = X509Certificate.CreateFromCertFile("keys/ecdsa_cert.pfx");

        return RunServerBase(
            async (sp, raw, ct) =>
            {
                this.Trace("start");

                this.Trace<string>("wrap {raw} into ssl stream", raw.GetFullId());
                await using var sslStream = new SslStream(new NetworkStream(raw, true), false);

                this.Trace("authenticate as server");
                await sslStream.AuthenticateAsServerAsync(
                    cert,
                    clientCertificateRequired: false,
                    checkCertificateRevocation: true
                );

                this.Trace("create managed socket");
                var options = ServerSocketOptions.Default with { Mode = SocketMode };
                var logger = sp.Resolve<ILogger>();
                var socket = new ServerSocket(sslStream, options, logger, ct);

                this.Trace<string>("handle {socket}", socket.GetFullId());
                await handleSocket(socket, ct);

                this.Trace("done");
            }
        );
    }
}
