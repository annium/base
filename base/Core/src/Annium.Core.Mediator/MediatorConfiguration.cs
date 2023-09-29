using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mediator.Internal;
using Annium.Linq;

namespace Annium.Core.Mediator;

public class MediatorConfiguration
{
    internal static MediatorConfiguration Merge(params MediatorConfiguration[] configurations)
    {
        return new(
            configurations.SelectMany(c => c.Handlers).ToList(),
            configurations.SelectMany(c => c.Matches).ToList()
        );
    }

    internal IEnumerable<Handler> Handlers => _handlers;
    internal IEnumerable<Match> Matches => _matches;
    private readonly IList<Handler> _handlers;
    private readonly IList<Match> _matches;

    internal MediatorConfiguration(
    ) : this(
        new List<Handler>(),
        new List<Match>()
    )
    {
    }

    private MediatorConfiguration(
        IList<Handler> handlers,
        IList<Match> matches
    )
    {
        _handlers = handlers;
        _matches = matches;
    }

    public MediatorConfiguration AddHandler(Type handlerType)
    {
        // ensure type is pipe or final handler
        var interfaces = handlerType.GetInterfaces()
            .Where(i => i.IsGenericType)
            .ToArray();

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
            .Where(x =>
                x.RequestedType == match.RequestedType &&
                x.ExpectedType == match.ExpectedType
            )
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