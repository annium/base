using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Sockets.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Sockets.Tests.Internal;

public class ClientServerManagedSocketSslTests : ClientServerManagedSocketTestsBase
{
    public ClientServerManagedSocketSslTests(ITestOutputHelper outputHelper)
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
    public async Task Listen_Canceled()
    {
        this.Trace("start");

        await Listen_Canceled_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ClientClosed()
    {
        this.Trace("start");

        await Listen_ClientClosed_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ServerClosed()
    {
        this.Trace("start");

        await Listen_ServerClosed_Base();

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

    internal override async Task HandleConnectAsync(IClientManagedSocket socket, CancellationToken ct)
    {
        this.Trace("start");

        var authOptions = new SslClientAuthenticationOptions
        {
            RemoteCertificateValidationCallback = (_, _, _, _) => true,
        };

        await socket.ConnectAsync(EndPoint, authOptions, ct);

        this.Trace("done");
    }

    internal override IAsyncDisposable RunServer(Func<IServerManagedSocket, CancellationToken, Task> handleSocket)
    {
        var cert = X509Certificate.CreateFromCertFile("keys/ecdsa_cert.pfx");

        return RunServerBase(
            async (sp, raw, ct) =>
            {
                this.Trace("start");

                this.Trace<string>("wrap {raw} into ssl stream", raw.GetFullId());
                await using var sslStream = new SslStream(new NetworkStream(raw), false);

                this.Trace("authenticate as server");
                await sslStream.AuthenticateAsServerAsync(
                    cert,
                    clientCertificateRequired: false,
                    checkCertificateRevocation: true
                );

                this.Trace("create managed socket");
                var logger = sp.Resolve<ILogger>();
                var socket = new ServerManagedSocket(sslStream, SocketMode, logger, ct);

                this.Trace<string>("handle {socket}", socket.GetFullId());
                await handleSocket(socket, ct);

                this.Trace("done");
            }
        );
    }
}
