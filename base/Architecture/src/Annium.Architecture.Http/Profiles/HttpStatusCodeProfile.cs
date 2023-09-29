using System.Net;
using Annium.Architecture.Base;
using Annium.Core.Mapper;

namespace Annium.Architecture.Http.Profiles;

public class HttpStatusCodeProfile : Profile
{
    public HttpStatusCodeProfile()
    {
        Map<HttpStatusCode, OperationStatus>(x => Map(x));
    }

    private OperationStatus Map(HttpStatusCode x) => x switch
    {
        HttpStatusCode.BadRequest => OperationStatus.BadRequest,
        HttpStatusCode.Conflict   => OperationStatus.Conflict,
        HttpStatusCode.Forbidden  => OperationStatus.Forbidden,
        HttpStatusCode.NotFound   => OperationStatus.NotFound,
        HttpStatusCode.OK         => OperationStatus.Ok,
        _                         => OperationStatus.UncaughtError,
    };
}