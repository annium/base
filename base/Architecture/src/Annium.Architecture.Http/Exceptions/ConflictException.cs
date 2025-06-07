using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

/// <summary>
/// Exception thrown when a request conflicts with the current state of the resource
/// </summary>
public class ConflictException : HttpException
{
    /// <summary>
    /// Initializes a new instance of the ConflictException class
    /// </summary>
    /// <param name="result">The result containing conflict information</param>
    public ConflictException(IResultBase result)
        : base(result) { }
}
