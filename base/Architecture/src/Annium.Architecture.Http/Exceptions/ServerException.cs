using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

/// <summary>
/// Exception thrown when a server error occurs
/// </summary>
public class ServerException : HttpException
{
    /// <summary>
    /// Initializes a new instance of the ServerException class
    /// </summary>
    /// <param name="result">The result containing server error information</param>
    public ServerException(IResultBase result)
        : base(result) { }
}
