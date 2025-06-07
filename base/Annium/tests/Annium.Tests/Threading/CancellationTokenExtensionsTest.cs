using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Threading;
using Xunit;

namespace Annium.Tests.Threading;

/// <summary>
/// Contains unit tests for the CancellationTokenExtensions class.
/// </summary>
public class CancellationTokenExtensionsTest
{
    /// <summary>
    /// Verifies that awaiting a CancellationToken works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CancellationToken_Await_Works()
    {
        // arrange
        var cts = new CancellationTokenSource();
        cts.CancelAfter(10);

        // act
        await cts.Token;

        // assert
        cts.Token.IsCancellationRequested.IsTrue();
    }
}
