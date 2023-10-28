using System;
using System.IO;
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

public class MessagingManagedSocketSslTests : MessagingManagedSocketTestsBase
{
    public MessagingManagedSocketSslTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

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
    public async Task Send_Normal()
    {
        this.Trace("start");

        await Send_Normal_Base();

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

    protected override async Task<Stream> CreateClientStreamAsync(Socket socket)
    {
        var networkStream = new NetworkStream(socket);
        var sslStream = new SslStream(networkStream, false, ValidateServerCertificate, null);

        await sslStream.AuthenticateAsClientAsync(string.Empty);

        return sslStream;

        bool ValidateServerCertificate(
            object sender,
            X509Certificate? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors
        )
        {
            // by design, no ssl verification in tests (cause it will require valid SSL certificate)
            return true;
        }
    }

    internal override IAsyncDisposable RunServer(Func<IManagedSocket, CancellationToken, Task> handleSocket)
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
                var socket = new MessagingManagedSocket(sslStream, sp.Resolve<ILogger>());

                this.Trace<string>("handle {socket}", socket.GetFullId());
                await handleSocket(socket, ct);

                this.Trace("done");
            }
        );
    }
}
