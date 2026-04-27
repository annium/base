using System.Net;
using Annium.Architecture.Base;
using Annium.Architecture.Http.Profiles;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Http.Tests;

/// <summary>
/// Verifies the reverse mapping in <see cref="HttpStatusCodeProfile"/> — every newly added
/// HTTP code (502/503/504/500) translates to the matching <see cref="OperationStatus"/>.
/// </summary>
public class HttpStatusCodeProfileTests : TestBase
{
    /// <summary>
    /// Initializes the mapper container with the <see cref="HttpStatusCodeProfile"/>.
    /// </summary>
    /// <param name="outputHelper">xunit output helper</param>
    public HttpStatusCodeProfileTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile<HttpStatusCodeProfile>());
    }

    /// <summary>BadGateway → NetworkError.</summary>
    [Fact]
    public void Map_BadGateway_NetworkError() =>
        Mapper().Map<OperationStatus>(HttpStatusCode.BadGateway).Is(OperationStatus.NetworkError);

    /// <summary>Unauthorized → Unauthorized.</summary>
    [Fact]
    public void Map_Unauthorized_ReturnsUnauthorized() =>
        Mapper().Map<OperationStatus>(HttpStatusCode.Unauthorized).Is(OperationStatus.Unauthorized);

    /// <summary>ServiceUnavailable → Aborted.</summary>
    [Fact]
    public void Map_ServiceUnavailable_Aborted() =>
        Mapper().Map<OperationStatus>(HttpStatusCode.ServiceUnavailable).Is(OperationStatus.Aborted);

    /// <summary>GatewayTimeout → Timeout.</summary>
    [Fact]
    public void Map_GatewayTimeout_Timeout() =>
        Mapper().Map<OperationStatus>(HttpStatusCode.GatewayTimeout).Is(OperationStatus.Timeout);

    /// <summary>InternalServerError → UncaughtError.</summary>
    [Fact]
    public void Map_InternalServerError_UncaughtError() =>
        Mapper().Map<OperationStatus>(HttpStatusCode.InternalServerError).Is(OperationStatus.UncaughtError);

    /// <summary>
    /// Pre-existing entries — keep the smoke check so a regression is obvious.
    /// </summary>
    [Fact]
    public void Map_PreExistingEntries_StillWork()
    {
        var mapper = Mapper();
        mapper.Map<OperationStatus>(HttpStatusCode.OK).Is(OperationStatus.Ok);
        mapper.Map<OperationStatus>(HttpStatusCode.BadRequest).Is(OperationStatus.BadRequest);
        mapper.Map<OperationStatus>(HttpStatusCode.Forbidden).Is(OperationStatus.Forbidden);
        mapper.Map<OperationStatus>(HttpStatusCode.NotFound).Is(OperationStatus.NotFound);
        mapper.Map<OperationStatus>(HttpStatusCode.Conflict).Is(OperationStatus.Conflict);
    }

    private IMapper Mapper() => Get<IMapper>();
}
