using System;
using System.Reflection;

namespace Annium.Core.Mediator.Internal;

/// <summary>
/// Represents a single element in the mediator execution chain
/// </summary>
internal class ChainElement
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
    /// Handler's HandleAsync method, resolved lazily on first dispatch and memoized. The element's
    /// runtime parameter types are stable across requests, so resolving once (by parameter types,
    /// which also disambiguates handlers implementing multiple handler interfaces) avoids a
    /// reflective GetMethod lookup on every request.
    /// </summary>
    public MethodInfo? Handle { get; set; }

    /// <summary>
    /// Initializes a new chain element with a handler and an optional next delegate
    /// </summary>
    /// <param name="handler">Type of the handler service</param>
    /// <param name="next">Delegate to invoke the next element in the chain; null for the final element</param>
    public ChainElement(Type handler, Delegate? next = null)
    {
        Handler = handler;
        Next = next;
    }

    /// <summary>
    /// Returns a string representation of the handler type
    /// </summary>
    /// <returns>String representation of the handler type</returns>
    public override string ToString() => Handler.ToString();
}
