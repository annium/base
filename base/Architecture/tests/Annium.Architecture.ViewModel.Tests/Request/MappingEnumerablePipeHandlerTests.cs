using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.ViewModel.Internal.PipeHandlers.Request;
using Annium.Testing;
using Xunit;
using static Annium.Architecture.ViewModel.Tests.Request.RequestMappingFixtures;

namespace Annium.Architecture.ViewModel.Tests.Request;

/// <summary>
/// Verifies the request-side enumerable mapping pipe handler maps the incoming view-model
/// enumerable request to its underlying type via the mapper and forwards the mapped collection
/// to the next delegate, including the empty-collection edge case.
/// </summary>
public class MappingEnumerablePipeHandlerTests
{
    /// <summary>
    /// Handler maps the enumerable request via the mapper exactly once and forwards the mapped
    /// collection to next.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HandleAsync_WithNonEmptyRequest_InvokesMapperOnceAndForwardsMappedCollection()
    {
        // arrange
        IEnumerable<TestRequestOut> expectedOut = new[] { new TestRequestOut(), new TestRequestOut() };
        var mapper = new StubMapper(expectedOut);
        var handler = new MappingEnumerablePipeHandler<TestRequestIn, TestRequestOut, TestResponse>(
            mapper,
            new NullLogger()
        );

        IEnumerable<TestRequestOut>? receivedByNext = null;

        // act
        var response = await handler.HandleAsync(
            new[] { new TestRequestIn(), new TestRequestIn() },
            CancellationToken.None,
            (req, _) =>
            {
                receivedByNext = req;
                return Task.FromResult(new TestResponse());
            }
        );

        // assert: mapper called exactly once
        mapper.Invocations.Is(1);

        // assert: next received the mapped collection, not the original
        ReferenceEquals(receivedByNext, expectedOut).IsTrue();

        // assert: mapped collection has the expected element count
        receivedByNext!.Count().Is(2);

        // assert: handler returns the value produced by next (non-null response)
        (response is not null).IsTrue();
    }

    /// <summary>
    /// Handler maps an empty enumerable request and forwards the mapped (also empty) collection
    /// to next; the mapper is still invoked exactly once.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task HandleAsync_WithEmptyRequest_InvokesMapperOnceAndForwardsEmptyCollection()
    {
        // arrange
        IEnumerable<TestRequestOut> emptyOut = Enumerable.Empty<TestRequestOut>();
        var mapper = new StubMapper(emptyOut);
        var handler = new MappingEnumerablePipeHandler<TestRequestIn, TestRequestOut, TestResponse>(
            mapper,
            new NullLogger()
        );

        IEnumerable<TestRequestOut>? receivedByNext = null;

        // act
        var response = await handler.HandleAsync(
            Enumerable.Empty<TestRequestIn>(),
            CancellationToken.None,
            (req, _) =>
            {
                receivedByNext = req;
                return Task.FromResult(new TestResponse());
            }
        );

        // assert: mapper still invoked exactly once even for an empty source
        mapper.Invocations.Is(1);

        // assert: next received the (empty) mapped collection
        ReferenceEquals(receivedByNext, emptyOut).IsTrue();
        receivedByNext!.Count().Is(0);

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
        IEnumerable<TestRequestOut> mappedOut = new[] { new TestRequestOut() };
        var expectedResponse = new TestResponse();
        var mapper = new StubMapper(mappedOut);
        var handler = new MappingEnumerablePipeHandler<TestRequestIn, TestRequestOut, TestResponse>(
            mapper,
            new NullLogger()
        );

        // act
        var response = await handler.HandleAsync(
            new[] { new TestRequestIn() },
            CancellationToken.None,
            (_, _) => Task.FromResult(expectedResponse)
        );

        // assert
        ReferenceEquals(response, expectedResponse).IsTrue();
    }

}
