using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

public class ValidationException : HttpException
{
    public ValidationException(IResultBase result) : base(result)
    {
    }
}