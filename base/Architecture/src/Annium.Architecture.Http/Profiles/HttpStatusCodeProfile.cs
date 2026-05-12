using System.Net;
using Annium.Architecture.Base;
using Annium.Core.Mapper;

namespace Annium.Architecture.Http.Profiles;

/// <summary>
/// Mapping profile for converting HTTP status codes to operation statuses
/// </summary>
public class HttpStatusCodeProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the HttpStatusCodeProfile class
    /// </summary>
    public HttpStatusCodeProfile()
    {
        Map<HttpStatusCode, OperationStatus>(x => Map(x));
    }

    /// <summary>
    /// Maps an HTTP status code to an operation status. Common semantically-meaningful codes
    /// (422 Unprocessable, 429 Too Many Requests, 410 Gone) are mapped explicitly so they do
    /// not silently surface as <see cref="OperationStatus.UncaughtError"/>. Unrecognised codes
    /// still fall through to <see cref="OperationStatus.UncaughtError"/> as a defensive default.
    /// </summary>
    /// <param name="x">The HTTP status code to map</param>
    /// <returns>The corresponding operation status</returns>
    private OperationStatus Map(HttpStatusCode x) =>
        x switch
        {
            HttpStatusCode.OK => OperationStatus.Ok,
            HttpStatusCode.BadRequest => OperationStatus.BadRequest,
            HttpStatusCode.UnprocessableEntity => OperationStatus.BadRequest,
            HttpStatusCode.Unauthorized => OperationStatus.Unauthorized,
            HttpStatusCode.Forbidden => OperationStatus.Forbidden,
            HttpStatusCode.NotFound => OperationStatus.NotFound,
            HttpStatusCode.Gone => OperationStatus.NotFound,
            HttpStatusCode.Conflict => OperationStatus.Conflict,
            HttpStatusCode.TooManyRequests => OperationStatus.Aborted,
            HttpStatusCode.BadGateway => OperationStatus.NetworkError,
            HttpStatusCode.ServiceUnavailable => OperationStatus.Aborted,
            HttpStatusCode.GatewayTimeout => OperationStatus.Timeout,
            // 500 is listed explicitly (even though `_` would also catch it with the same mapping)
            // so the 500 → UncaughtError intent survives any future refactor of the `_` arm.
            HttpStatusCode.InternalServerError => OperationStatus.UncaughtError,
            _ => OperationStatus.UncaughtError,
        };
}
