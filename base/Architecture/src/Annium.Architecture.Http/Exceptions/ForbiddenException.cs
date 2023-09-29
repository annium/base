using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

public class ForbiddenException : HttpException
{
    public ForbiddenException(IResultBase result) : base(result)
    {
    }
}