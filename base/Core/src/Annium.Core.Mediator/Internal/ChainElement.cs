using System;

namespace Annium.Core.Mediator.Internal;

/// <summary>
/// Represents a single element in the mediator execution chain
/// </summary>
internal record ChainElement
{
    /// <summary>
    /// Type of the handler service for this chain element
    /// </summary>
    public Type Handler { get; }

    /// <summary>
    /// Delegate function to invoke the next element in the chain, if any
    /// </summary>
    public Delegate? Next { get; }

    /// <summary>
    /// Initializes a new chain element with a handler (final element)
    /// </summary>
    /// <param name="handler">Type of the handler service</param>
    public ChainElement(Type handler)
    {
        Handler = handler;
    }

    /// <summary>
    /// Initializes a new chain element with a handler and next delegate
    /// </summary>
    /// <param name="handler">Type of the handler service</param>
    /// <param name="next">Delegate to invoke the next element in the chain</param>
    public ChainElement(Type handler, Delegate next)
        : this(handler)
    {
        Next = next;
    }

    /// <summary>
    /// Returns a string representation of the handler type
    /// </summary>
    /// <returns>String representation of the handler type</returns>
    public override string ToString() => Handler.ToString();
}
