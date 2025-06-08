using Annium.Data.Operations;

namespace Annium.Architecture.Http.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : HttpException
{
    /// <summary>
    /// Initializes a new instance of the NotFoundException class
    /// </summary>
    /// <param name="result">The result containing not found information</param>
    public NotFoundException(IResultBase result)
        : base(result) { }
}
