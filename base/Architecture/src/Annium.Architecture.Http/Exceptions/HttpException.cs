using System;
using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

/// <summary>
/// Base exception class for HTTP-related exceptions
/// </summary>
public class HttpException : Exception
{
    /// <summary>
    /// Gets the result associated with this HTTP exception
    /// </summary>
    public IResultBase Result { get; }

    /// <summary>
    /// Initializes a new instance of the HttpException class
    /// </summary>
    /// <param name="result">The result associated with the exception</param>
    protected HttpException(IResultBase result)
    {
        Result = result;
    }
}
