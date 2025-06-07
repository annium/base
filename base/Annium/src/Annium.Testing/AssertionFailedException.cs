using System;

namespace Annium.Testing;

/// <summary>
/// The exception that is thrown when an assertion fails.
/// </summary>
public class AssertionFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssertionFailedException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public AssertionFailedException(string message)
        : base(message) { }
}
