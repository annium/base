using System;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

public class HttpException : Exception
{
    public IResultBase Result { get; }

    protected HttpException(IResultBase result)
    {
        Result = result;
    }
}