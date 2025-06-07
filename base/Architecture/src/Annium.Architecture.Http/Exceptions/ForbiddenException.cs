using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

/// <summary>
/// Exception thrown when access to a resource is forbidden
/// </summary>
public class ForbiddenException : HttpException
{
    /// <summary>
    /// Initializes a new instance of the ForbiddenException class
    /// </summary>
    /// <param name="result">The result containing forbidden access information</param>
    public ForbiddenException(IResultBase result)
        : base(result) { }
}
