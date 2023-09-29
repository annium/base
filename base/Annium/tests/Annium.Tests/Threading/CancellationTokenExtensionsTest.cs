using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Annium.Threading;
using Xunit;

namespace Annium.Tests.Threading;

public class CancellationTokenExtensionsTest
{
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