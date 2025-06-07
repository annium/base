using System;

namespace Annium.Core.Mediator.Internal;

/// <summary>
/// Represents a type mapping for mediator request/response resolution
/// </summary>
internal record Match
{
    /// <summary>
    /// Type that was originally requested
    /// </summary>
    public Type RequestedType { get; }

    /// <summary>
    /// Type that was expected in the request
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    /// Type that should be used for resolution
    /// </summary>
    public Type ResolvedType { get; }

    /// <summary>
    /// Initializes a new type match definition
    /// </summary>
    /// <param name="requestedType">Type that was originally requested</param>
    /// <param name="expectedType">Type that was expected</param>
    /// <param name="resolvedType">Type that should be used for resolution</param>
    internal Match(Type requestedType, Type expectedType, Type resolvedType)
    {
        RequestedType = requestedType;
        ExpectedType = expectedType;
        ResolvedType = resolvedType;
    }

    /// <summary>
    /// Returns a string representation of the type match
    /// </summary>
    /// <returns>String representation showing the type mapping</returns>
    public override string ToString() =>
        $"{RequestedType.FriendlyName()} -> {ResolvedType.FriendlyName()} ({ExpectedType.FriendlyName()})";
}
