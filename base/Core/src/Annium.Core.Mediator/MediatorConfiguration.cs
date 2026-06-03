using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mediator.Internal;
using Annium.Linq;

namespace Annium.Core.Mediator;

/// <summary>
/// Configuration for mediator request handlers and type matches
/// </summary>
public class MediatorConfiguration
{
    /// <summary>
    /// Merges multiple mediator configurations into a single configuration
    /// </summary>
    /// <param name="configurations">Configurations to merge</param>
    /// <returns>Merged configuration containing all handlers and matches</returns>
    internal static MediatorConfiguration Merge(params MediatorConfiguration[] configurations)
    {
        var handlers = configurations.SelectMany(c => c.Handlers).ToList();

        // AddMatch dedups/ambiguity-checks matches within a single config; the same must hold across
        // merged configs, otherwise duplicate (RequestedType, ExpectedType) entries make ChainBuilder's
        // SingleOrDefault throw a cryptic "Sequence contains more than one element".
        var matches = new List<Match>();
        foreach (var match in configurations.SelectMany(c => c.Matches))
        {
            var existing = matches.FirstOrDefault(x =>
                x.RequestedType == match.RequestedType && x.ExpectedType == match.ExpectedType
            );
            if (existing is null)
            {
                matches.Add(match);
                continue;
            }

            if (existing.ResolvedType != match.ResolvedType)
                throw new InvalidOperationException(
                    $"Match {match} conflicts with {existing}: the same requested/expected type pair resolves to different types across configurations"
                );
            // identical match registered in another configuration — skip the duplicate
        }

        return new(handlers, matches);
    }

    /// <summary>
    /// Collection of registered request handlers
    /// </summary>
    internal IReadOnlyList<Handler> Handlers => _handlers;

    /// <summary>
    /// Collection of registered type matches
    /// </summary>
    internal IReadOnlyList<Match> Matches => _matches;

    /// <summary>
    /// Internal storage for request handlers
    /// </summary>
    private readonly List<Handler> _handlers;

    /// <summary>
    /// Internal storage for type matches
    /// </summary>
    private readonly List<Match> _matches;

    /// <summary>
    /// Initializes a new empty mediator configuration
    /// </summary>
    internal MediatorConfiguration()
        : this(new List<Handler>(), new List<Match>()) { }

    /// <summary>
    /// Initializes a new mediator configuration with specified handlers and matches
    /// </summary>
    /// <param name="handlers">List of request handlers</param>
    /// <param name="matches">List of type matches</param>
    private MediatorConfiguration(List<Handler> handlers, List<Match> matches)
    {
        _handlers = handlers;
        _matches = matches;
    }

    /// <summary>
    /// Adds a request handler type to the configuration
    /// </summary>
    /// <param name="handlerType">Type implementing pipe or final request handler interface</param>
    /// <returns>This configuration instance for method chaining</returns>
    public MediatorConfiguration AddHandler(Type handlerType)
    {
        // ensure type is pipe or final handler
        var interfaces = handlerType.GetInterfaces().Where(i => i.IsGenericType).ToArray();

        var isRegistered = false;

        foreach (var serviceType in interfaces.Where(i => i.GetGenericTypeDefinition() == Constants.PipeHandlerType))
        {
            var args = serviceType.GetGenericArguments();
            _handlers.Add(new Handler(handlerType, args[0], args[1], args[2], args[3]));
            isRegistered = true;
        }

        foreach (var serviceType in interfaces.Where(i => i.GetGenericTypeDefinition() == Constants.FinalHandlerType))
        {
            var args = serviceType.GetGenericArguments();
            _handlers.Add(new Handler(handlerType, args[0], null, null, args[1]));
            isRegistered = true;
        }

        if (isRegistered)
            return this;

        throw new InvalidOperationException(
            $"To register {handlerType.FriendlyName()} as Mediator request handler, it must implement {Constants.PipeHandlerType.FriendlyName()} or {Constants.FinalHandlerType.FriendlyName()}"
        );
    }

    /// <summary>
    /// Adds a type match for request/response resolution
    /// </summary>
    /// <param name="requestType">Type that was originally requested</param>
    /// <param name="expectedType">Type that was expected</param>
    /// <param name="resolvedType">Type that should be used for resolution</param>
    /// <returns>This configuration instance for method chaining</returns>
    public MediatorConfiguration AddMatch(Type requestType, Type expectedType, Type resolvedType)
    {
        ThrowIfGeneric(requestType, "Requested");
        ThrowIfGeneric(expectedType, "Expected");
        ThrowIfGeneric(resolvedType, "Resolved");

        if (!expectedType.IsAssignableFrom(resolvedType))
            throw new InvalidOperationException(
                $"Resolved type {resolvedType.FriendlyName()} must be assignable to expected type {expectedType.FriendlyName()}"
            );

        var match = new Match(requestType, expectedType, resolvedType);
        var duplicates = _matches
            .Where(x => x.RequestedType == match.RequestedType && x.ExpectedType == match.ExpectedType)
            .ToArray();

        if (duplicates.Length == 0)
            _matches.Add(match);
        // if duplicates - throw or skip if same
        else
        {
            var ambiguities = duplicates
                .Where(x => x.ResolvedType != match.ResolvedType)
                .Select(x => x.ResolvedType.FriendlyName())
                .ToArray();
            if (ambiguities.Length > 0)
                throw new InvalidOperationException($"Match {match} is also resolved in: {ambiguities.Join(", ")}");
        }

        return this;
    }

    /// <summary>
    /// Throws if the given type is an open generic (a generic type parameter or contains unbound generic parameters)
    /// </summary>
    /// <param name="type">Type to validate</param>
    /// <param name="label">Role label used in the error message (e.g. "Requested", "Expected", "Resolved")</param>
    private static void ThrowIfGeneric(Type type, string label)
    {
        if (type.IsGenericTypeParameter || type.ContainsGenericParameters)
            throw new InvalidOperationException(
                $"{label} type {type.FriendlyName()} can't be registered in request/response match, because it is generic"
            );
    }
}
