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
        return new(
            configurations.SelectMany(c => c.Handlers).ToList(),
            configurations.SelectMany(c => c.Matches).ToList()
        );
    }

    /// <summary>
    /// Collection of registered request handlers
    /// </summary>
    internal IEnumerable<Handler> Handlers => _handlers;

    /// <summary>
    /// Collection of registered type matches
    /// </summary>
    internal IEnumerable<Match> Matches => _matches;

    /// <summary>
    /// Internal storage for request handlers
    /// </summary>
    private readonly IList<Handler> _handlers;

    /// <summary>
    /// Internal storage for type matches
    /// </summary>
    private readonly IList<Match> _matches;

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
    private MediatorConfiguration(IList<Handler> handlers, IList<Match> matches)
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
        if (requestType.IsGenericTypeParameter || requestType.ContainsGenericParameters)
            throw new InvalidOperationException(
                $"Requested type {requestType.FriendlyName()} can't be registered in request/response match, because it is generic"
            );

        if (expectedType.IsGenericTypeParameter || expectedType.ContainsGenericParameters)
            throw new InvalidOperationException(
                $"Expected type {expectedType.FriendlyName()} can't be registered in request/response match, because it is generic"
            );

        if (resolvedType.IsGenericTypeParameter || resolvedType.ContainsGenericParameters)
            throw new InvalidOperationException(
                $"Resolved type {expectedType.FriendlyName()} can't be registered in request/response match, because it is generic"
            );

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
}
