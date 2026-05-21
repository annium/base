using Annium.Architecture.Base;
using Annium.Architecture.Http.Exceptions;
using Annium.Architecture.Http.Internal.PipeHandlers;
using Annium.Data.Operations;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Http.Tests;

/// <summary>
/// Verifies the total mapping in <see cref="HttpStatusPipeHandlerBase{TRequest,TResponseIn,TResponseOut}.HandleStatus"/>
/// — every <see cref="OperationStatus"/> member must throw the expected exception type (or no-op for Ok).
/// </summary>
public class HttpStatusPipeHandlerBaseTests
{
    /// <summary>Ok must not throw.</summary>
    [Fact]
    public void HandleStatus_Ok_DoesNotThrow()
    {
        new TestPipeHandler().Invoke(OperationStatus.Ok);
    }

    /// <summary>BadRequest → ValidationException.</summary>
    [Fact]
    public void HandleStatus_BadRequest_ThrowsValidationException()
    {
        Wrap.It(() => new TestPipeHandler().Invoke(OperationStatus.BadRequest)).Throws<ValidationException>();
    }

    /// <summary>Unauthorized → UnauthorizedException (HTTP 401).</summary>
    [Fact]
    public void HandleStatus_Unauthorized_ThrowsUnauthorizedException()
    {
        Wrap.It(() => new TestPipeHandler().Invoke(OperationStatus.Unauthorized)).Throws<UnauthorizedException>();
    }

    /// <summary>Forbidden → ForbiddenException.</summary>
    [Fact]
    public void HandleStatus_Forbidden_ThrowsForbiddenException()
    {
        Wrap.It(() => new TestPipeHandler().Invoke(OperationStatus.Forbidden)).Throws<ForbiddenException>();
    }

    /// <summary>NotFound → NotFoundException.</summary>
    [Fact]
    public void HandleStatus_NotFound_ThrowsNotFoundException()
    {
        Wrap.It(() => new TestPipeHandler().Invoke(OperationStatus.NotFound)).Throws<NotFoundException>();
    }

    /// <summary>Conflict → ConflictException.</summary>
    [Fact]
    public void HandleStatus_Conflict_ThrowsConflictException()
    {
        Wrap.It(() => new TestPipeHandler().Invoke(OperationStatus.Conflict)).Throws<ConflictException>();
    }

    /// <summary>NetworkError → BadGatewayException (HTTP 502).</summary>
    [Fact]
    public void HandleStatus_NetworkError_ThrowsBadGatewayException()
    {
        Wrap.It(() => new TestPipeHandler().Invoke(OperationStatus.NetworkError)).Throws<BadGatewayException>();
    }

    /// <summary>Aborted → ServiceUnavailableException (HTTP 503).</summary>
    [Fact]
    public void HandleStatus_Aborted_ThrowsServiceUnavailableException()
    {
        Wrap.It(() => new TestPipeHandler().Invoke(OperationStatus.Aborted)).Throws<ServiceUnavailableException>();
    }

    /// <summary>Timeout → GatewayTimeoutException (HTTP 504).</summary>
    [Fact]
    public void HandleStatus_Timeout_ThrowsGatewayTimeoutException()
    {
        Wrap.It(() => new TestPipeHandler().Invoke(OperationStatus.Timeout)).Throws<GatewayTimeoutException>();
    }

    /// <summary>UncaughtError → ServerException (HTTP 500).</summary>
    [Fact]
    public void HandleStatus_UncaughtError_ThrowsServerException()
    {
        Wrap.It(() => new TestPipeHandler().Invoke(OperationStatus.UncaughtError)).Throws<ServerException>();
    }

    /// <summary>Unknown future enum member → ServerException fallback.</summary>
    [Fact]
    public void HandleStatus_UnknownStatus_ThrowsServerException()
    {
        var unknown = (OperationStatus)int.MaxValue;
        Wrap.It(() => new TestPipeHandler().Invoke(unknown)).Throws<ServerException>();
    }

    /// <summary>
    /// Minimal subclass that exposes the protected <c>HandleStatus</c> for direct invocation.
    /// </summary>
    private sealed class TestPipeHandler : HttpStatusPipeHandlerBase<object, object, object>
    {
        public void Invoke(OperationStatus status) => HandleStatus(status, Result.Create());

        protected override object GetResponse(object response) => response;
    }
}
