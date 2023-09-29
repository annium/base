using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

public class ConflictException : HttpException
{
    public ConflictException(IResultBase result) : base(result)
    {
    }
}