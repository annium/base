using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

public class NotFoundException : HttpException
{
    public NotFoundException(IResultBase result) : base(result)
    {
    }
}