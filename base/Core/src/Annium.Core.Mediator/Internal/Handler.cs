using System;

namespace Annium.Core.Mediator.Internal;

/// <summary>
/// Represents a request handler definition with its type mappings
/// </summary>
internal record Handler
{
    /// <summary>
    /// Implementation type of the handler
    /// </summary>
    public Type Implementation { get; }

    /// <summary>
    /// Input request type handled by this handler
    /// </summary>
    public Type RequestIn { get; }

    /// <summary>
    /// Output request type produced by this handler (for pipe handlers)
    /// </summary>
    public Type? RequestOut { get; }

    /// <summary>
    /// Input response type expected by this handler (for pipe handlers)
    /// </summary>
    public Type? ResponseIn { get; }

    /// <summary>
    /// Output response type produced by this handler
    /// </summary>
    public Type ResponseOut { get; }

    /// <summary>
    /// Initializes a new handler definition
    /// </summary>
    /// <param name="implementation">Implementation type of the handler</param>
    /// <param name="requestIn">Input request type</param>
    /// <param name="requestOut">Output request type (for pipe handlers)</param>
    /// <param name="responseIn">Input response type (for pipe handlers)</param>
    /// <param name="responseOut">Output response type</param>
    internal Handler(Type implementation, Type requestIn, Type? requestOut, Type? responseIn, Type responseOut)
    {
        Implementation = implementation;
        RequestIn = requestIn;
        RequestOut = requestOut;
        ResponseIn = responseIn;
        ResponseOut = responseOut;
    }

    /// <summary>
    /// Returns a string representation of the handler implementation type
    /// </summary>
    /// <returns>String representation of the implementation type</returns>
    public override string ToString() => Implementation.ToString();
}
