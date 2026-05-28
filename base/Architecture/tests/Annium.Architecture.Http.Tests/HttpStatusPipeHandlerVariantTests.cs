using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Architecture.Http.Exceptions;
using Annium.Architecture.Http.Internal.PipeHandlers.Request;
using Annium.Architecture.Http.Internal.PipeHandlers.RequestResponse;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Http.Tests;

/// <summary>
/// Covers the two concrete <c>HttpStatusPipeHandler</c> variants that are not exercised
/// by <see cref="HttpStatusPipeHandlerBaseTests"/>:
/// <list type="bullet">
///   <item><description>
///     <c>RequestResponse.HttpStatusPipeHandler</c> — the Status=Ok / Data=null guard that
///     surfaces as <see cref="ServerException"/> (HTTP 500).
///   </description></item>
///   <item><description>
///     <c>Request.HttpStatusPipeHandler</c> — the request-only GetResponse path: non-Ok
///     statuses throw the matching exception, and Ok returns a joined result.
///   </description></item>
/// </list>
/// </summary>
public class HttpStatusPipeHandlerVariantTests
{
    // -------------------------------------------------------------------------
    // RequestResponse variant
    // -------------------------------------------------------------------------

    /// <summary>
    /// When the upstream handler returns Status=Ok with a non-null data payload the
    /// handler must return a joined <see cref="IResult{TResponse}"/> without throwing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RequestResponse_GetResponse_OkWithData_ReturnsJoinedResult()
    {
        var handler = new RequestResponseHandler();
        var upstream = Result.Status<OperationStatus, string>(OperationStatus.Ok, "hello");

        var result = await handler.HandleAsync(
            new object(),
            CancellationToken.None,
            (_, _) => Task.FromResult<IStatusResult<OperationStatus, string>>(upstream)
        );

        result.IsOk.IsTrue();
        result.Data.Is("hello");
    }

    /// <summary>
    /// When the upstream handler returns Status=Ok but <c>Data</c> is null (a programming
    /// error — TResponse has no <c>notnull</c> constraint) the handler must throw
    /// <see cref="ServerException"/> rather than propagate a null-data result downstream.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RequestResponse_GetResponse_OkWithNullData_ThrowsServerException()
    {
        var handler = new RequestResponseHandler();
        // Force null into the Data slot of a string-typed result to simulate a
        // misbehaving upstream that returns Ok with no data.
        var upstream = Result.Status<OperationStatus, string>(OperationStatus.Ok, null!);

        await Wrap.It(async () =>
            await handler.HandleAsync(
                new object(),
                CancellationToken.None,
                (_, _) => Task.FromResult<IStatusResult<OperationStatus, string>>(upstream)
            )
        ).ThrowsAsync<ServerException>();
    }

    /// <summary>
    /// When the upstream handler returns a non-Ok status the handler must throw the
    /// matching HTTP exception — validating that <c>HandleStatus</c> is wired into
    /// <c>GetResponse</c> for the request-response variant.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task RequestResponse_GetResponse_NonOkStatus_ThrowsMatchingException()
    {
        var handler = new RequestResponseHandler();
        var upstream = Result.Status<OperationStatus, string>(OperationStatus.NotFound, string.Empty);

        await Wrap.It(async () =>
            await handler.HandleAsync(
                new object(),
                CancellationToken.None,
                (_, _) => Task.FromResult<IStatusResult<OperationStatus, string>>(upstream)
            )
        ).ThrowsAsync<NotFoundException>();
    }

    // -------------------------------------------------------------------------
    // Request variant
    // -------------------------------------------------------------------------

    /// <summary>
    /// When the upstream handler returns Status=Ok the request-only handler must return
    /// a joined <see cref="IResult"/> without throwing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Request_GetResponse_Ok_ReturnsJoinedResult()
    {
        var handler = new RequestOnlyHandler();
        var upstream = Result.Status<OperationStatus>(OperationStatus.Ok);

        var result = await handler.HandleAsync(
            new object(),
            CancellationToken.None,
            (_, _) => Task.FromResult<IStatusResult<OperationStatus>>(upstream)
        );

        result.IsOk.IsTrue();
    }

    /// <summary>
    /// When the upstream handler returns a non-Ok status the request-only handler must
    /// throw the matching HTTP exception.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Request_GetResponse_NonOkStatus_ThrowsMatchingException()
    {
        var handler = new RequestOnlyHandler();
        var upstream = Result.Status<OperationStatus>(OperationStatus.BadRequest);

        await Wrap.It(async () =>
            await handler.HandleAsync(
                new object(),
                CancellationToken.None,
                (_, _) => Task.FromResult<IStatusResult<OperationStatus>>(upstream)
            )
        ).ThrowsAsync<ValidationException>();
    }

    /// <summary>
    /// Every non-Ok status must produce a distinct matching exception. This table test
    /// covers all remaining statuses for the request-only path.
    /// </summary>
    /// <param name="status">The operation status to exercise.</param>
    /// <param name="expectedExceptionType">The HTTP exception type the handler must throw.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Theory]
    [InlineData(OperationStatus.Unauthorized, typeof(UnauthorizedException))]
    [InlineData(OperationStatus.Forbidden, typeof(ForbiddenException))]
    [InlineData(OperationStatus.NotFound, typeof(NotFoundException))]
    [InlineData(OperationStatus.Conflict, typeof(ConflictException))]
    [InlineData(OperationStatus.NetworkError, typeof(BadGatewayException))]
    [InlineData(OperationStatus.Aborted, typeof(ServiceUnavailableException))]
    [InlineData(OperationStatus.Timeout, typeof(GatewayTimeoutException))]
    [InlineData(OperationStatus.UncaughtError, typeof(ServerException))]
    public async Task Request_GetResponse_StatusMappings_ThrowExpectedException(
        OperationStatus status,
        Type expectedExceptionType
    )
    {
        var handler = new RequestOnlyHandler();
        var upstream = Result.Status<OperationStatus>(status);

        Exception? caught = null;
        try
        {
            await handler.HandleAsync(
                new object(),
                CancellationToken.None,
                (_, _) => Task.FromResult<IStatusResult<OperationStatus>>(upstream)
            );
        }
        catch (Exception ex)
        {
            caught = ex;
        }

        (caught is not null).IsTrue();
        caught!.GetType().Is(expectedExceptionType);
    }

    // -------------------------------------------------------------------------
    // Test-local concrete handler wrappers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Exposes <c>HandleAsync</c> for <c>object</c> →
    /// <c>IStatusResult&lt;OperationStatus, string&gt;</c> → <c>IResult&lt;string&gt;</c>
    /// so the tests can drive the concrete <see cref="HttpStatusPipeHandler{TRequest,TResponse}"/>
    /// end-to-end without registering the handler in a mediator pipeline.
    /// </summary>
    private sealed class RequestResponseHandler
        : HttpStatusPipeHandler<object, string>
    {
        // No additional members needed; HandleAsync is public on the base class.
    }

    /// <summary>
    /// Exposes <c>HandleAsync</c> for <c>object</c> →
    /// <c>IStatusResult&lt;OperationStatus&gt;</c> → <c>IResult</c>
    /// so the tests can drive the concrete <see cref="HttpStatusPipeHandler{TRequest}"/>
    /// end-to-end without registering the handler in a mediator pipeline.
    /// </summary>
    private sealed class RequestOnlyHandler
        : HttpStatusPipeHandler<object>
    {
        // No additional members needed; HandleAsync is public on the base class.
    }
}
