using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Annium.Net;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Net;

/// <summary>
/// Contains unit tests for <see cref="IPEndPoints"/> (review T10 — previously 0% covered;
/// also exercises the IPv4-resolution fix from review bug B1).
/// </summary>
public class IPEndPointsTests
{
    /// <summary>
    /// Verifies that <c>ParseAsync</c> throws <see cref="System.ArgumentOutOfRangeException"/> when
    /// <c>defaultPort</c> is negative.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseAsync_InvalidDefaultPort_Negative_Throws()
    {
        await Wrap
            .It(async () => await IPEndPoints.ParseAsync("127.0.0.1:80", defaultPort: -1))
            .ThrowsAsync<System.ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <c>ParseAsync</c> throws <see cref="System.ArgumentOutOfRangeException"/> when
    /// <c>defaultPort</c> is at or above 65536.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseAsync_InvalidDefaultPort_TooLarge_Throws()
    {
        await Wrap
            .It(async () => await IPEndPoints.ParseAsync("127.0.0.1:80", defaultPort: 65536))
            .ThrowsAsync<System.ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <c>ParseAsync</c> parses an IPv4 literal with port.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ParseAsync_IpLiteral_WithPort_ParsesCorrectly()
    {
        var endpoint = await IPEndPoints.ParseAsync("127.0.0.1:8080", ct: TestContext.Current.CancellationToken);

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
        var endpoint = await IPEndPoints.ParseAsync("127.0.0.1", defaultPort: 9000, ct: TestContext.Current.CancellationToken);

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
        var endpoint = await IPEndPoints.ParseAsync("localhost:1234", ct: TestContext.Current.CancellationToken);

        endpoint.Address.AddressFamily.Is(AddressFamily.InterNetwork);
        endpoint.Port.Is(1234);
    }
}
