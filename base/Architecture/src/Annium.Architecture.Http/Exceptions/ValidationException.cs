using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

/// <summary>
/// Exception thrown when request validation fails
/// </summary>
public class ValidationException : HttpException
{
    /// <summary>
    /// Initializes a new instance of the ValidationException class
    /// </summary>
    /// <param name="result">The result containing validation error information</param>
    public ValidationException(IResultBase result)
        : base(result) { }
}
