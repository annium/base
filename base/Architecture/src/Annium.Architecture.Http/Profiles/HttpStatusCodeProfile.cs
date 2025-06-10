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
    /// Maps an HTTP status code to an operation status
    /// </summary>
    /// <param name="x">The HTTP status code to map</param>
    /// <returns>The corresponding operation status</returns>
    private OperationStatus Map(HttpStatusCode x) =>
        x switch
        {
            HttpStatusCode.BadRequest => OperationStatus.BadRequest,
            HttpStatusCode.Conflict => OperationStatus.Conflict,
            HttpStatusCode.Forbidden => OperationStatus.Forbidden,
            HttpStatusCode.NotFound => OperationStatus.NotFound,
            HttpStatusCode.OK => OperationStatus.Ok,
            _ => OperationStatus.UncaughtError,
        };
}
