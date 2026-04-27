using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

/// <summary>
/// Exception thrown when a request requires authentication that is missing, invalid, or expired (HTTP 401).
/// </summary>
public class UnauthorizedException : HttpException
{
    /// <summary>
    /// Initializes a new instance of the UnauthorizedException class.
    /// </summary>
    /// <param name="result">The result containing authentication failure information</param>
    public UnauthorizedException(IResultBase result)
        : base(result) { }
}
