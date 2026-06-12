using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Annium.Net;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Net;

/// <summary>
/// Contains unit tests for <see cref="IpEndPoints"/> (review T10 — previously 0% covered;
/// also exercises the IPv4-resolution fix from review bug B1).
/// </summary>
public class IpEndPointsTests
{
    /// <summary>
    /// Verifies that <c>ParseAsync</c> throws <see cref="System.ArgumentOutOfRangeException"/> when
    /// <c>defaultPort</c> is negative.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseAsync_InvalidDefaultPort_Negative_Throws()
    {
        await Wrap.It(async () => await IpEndPoints.ParseAsync("127.0.0.1:80", defaultPort: -1))
            .ThrowsAsync<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <c>ParseAsync</c> throws <see cref="System.ArgumentOutOfRangeException"/> when
    /// <c>defaultPort</c> is at or above 65536.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseAsync_InvalidDefaultPort_TooLarge_Throws()
    {
        await Wrap.It(async () => await IpEndPoints.ParseAsync("127.0.0.1:80", defaultPort: 65536))
            .ThrowsAsync<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <c>ParseAsync</c> parses an IPv4 literal with port.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseAsync_IpLiteral_WithPort_ParsesCorrectly()
    {
        var endpoint = await IpEndPoints.ParseAsync("127.0.0.1:8080", ct: TestContext.Current.CancellationToken);

        endpoint.Address.Is(IPAddress.Loopback);
        endpoint.Port.Is(8080);
    }

    /// <summary>
    /// Verifies that <c>ParseAsync</c> uses the supplied <c>defaultPort</c> when the input has no port.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseAsync_IpLiteral_NoPort_UsesDefaultPort()
    {
        var endpoint = await IpEndPoints.ParseAsync(
            "127.0.0.1",
            defaultPort: 9000,
            ct: TestContext.Current.CancellationToken
        );

        endpoint.Address.Is(IPAddress.Loopback);
        endpoint.Port.Is(9000);
    }

    /// <summary>
    /// Verifies that <c>ParseAsync</c> resolves a hostname (localhost) to an IPv4 address.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseAsync_Localhost_ResolvesToIPv4()
    {
        var endpoint = await IpEndPoints.ParseAsync("localhost:1234", ct: TestContext.Current.CancellationToken);

        endpoint.Address.AddressFamily.Is(AddressFamily.InterNetwork);
        endpoint.Port.Is(1234);
    }

    /// <summary>
    /// Verifies that <c>ParseAsync</c> falls back to loopback (127.0.0.1) with the supplied default port
    /// when the input cannot be parsed as a URI (e.g. contains a literal bracket that breaks URI syntax).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseAsync_UnparseableInput_FallsBackToLoopbackWithDefaultPort()
    {
        // "[::invalid" is not a valid URI host segment — Uri.TryCreate rejects it, so the method
        // returns IPAddress.Loopback with the provided defaultPort.
        var endpoint = await IpEndPoints.ParseAsync(
            "[::invalid",
            defaultPort: 7777,
            ct: TestContext.Current.CancellationToken
        );

        endpoint.Address.Is(IPAddress.Loopback);
        endpoint.Port.Is(7777);
    }

    /// <summary>
    /// Verifies that <c>ParseAsync</c> falls back to loopback (127.0.0.1) when the numeric host
    /// is syntactically valid as a URI but cannot be parsed as an IP address (e.g. an out-of-range octet),
    /// and uses the supplied default port.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseAsync_InvalidIpOctet_FallsBackToLoopbackWithDefaultPort()
    {
        // "999.999.999.999" is accepted by Uri.TryCreate (no letters → DNS path skipped),
        // but IPAddress.TryParse rejects it → final loopback fallback.
        var endpoint = await IpEndPoints.ParseAsync(
            "999.999.999.999:4321",
            defaultPort: 0,
            ct: TestContext.Current.CancellationToken
        );

        endpoint.Address.Is(IPAddress.Loopback);
        endpoint.Port.Is(4321);
    }
}
