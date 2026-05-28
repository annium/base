using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.ViewModel.Internal.PipeHandlers.Request;
using Annium.Testing;
using Xunit;
using static Annium.Architecture.ViewModel.Tests.Request.RequestMappingFixtures;

namespace Annium.Architecture.ViewModel.Tests.Request;

/// <summary>
/// Verifies the request-side single mapping pipe handler maps the incoming view-model request
/// to its underlying type via the mapper and forwards the mapped value to the next delegate.
/// </summary>
public class MappingSinglePipeHandlerTests
{
    /// <summary>
    /// Handler maps the request via the mapper exactly once and forwards the mapped value to next.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HandleAsync_WithRequest_InvokesMapperOnceAndForwardsMappedValue()
    {
        // arrange
        var expectedOut = new TestRequestOut();
        var mapper = new StubMapper(expectedOut);
        var handler = new MappingSinglePipeHandler<TestRequestIn, TestRequestOut, TestResponse>(
            mapper,
            new NullLogger()
        );

        TestRequestOut? receivedByNext = null;

        // act
        var response = await handler.HandleAsync(
            new TestRequestIn(),
            CancellationToken.None,
            (req, _) =>
            {
                receivedByNext = req;
                return Task.FromResult(new TestResponse());
            }
        );

        // assert: mapper called exactly once
        mapper.Invocations.Is(1);

        // assert: next received the mapped value, not the original
        ReferenceEquals(receivedByNext, expectedOut).IsTrue();

        // assert: handler returns the value produced by next (non-null response)
        (response is not null).IsTrue();
    }

    /// <summary>
    /// Handler returns the response produced by the next delegate unchanged.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HandleAsync_WithRequest_ReturnsResponseFromNext()
    {
        // arrange
        var expectedOut = new TestRequestOut();
        var expectedResponse = new TestResponse();
        var mapper = new StubMapper(expectedOut);
        var handler = new MappingSinglePipeHandler<TestRequestIn, TestRequestOut, TestResponse>(
            mapper,
            new NullLogger()
        );

        // act
        var response = await handler.HandleAsync(
            new TestRequestIn(),
            CancellationToken.None,
            (_, _) => Task.FromResult(expectedResponse)
        );

        // assert
        ReferenceEquals(response, expectedResponse).IsTrue();
    }

}
