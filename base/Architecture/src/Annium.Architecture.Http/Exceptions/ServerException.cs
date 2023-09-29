using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

public class ServerException : HttpException
{
    public ServerException(IResultBase result) : base(result)
    {
    }
}