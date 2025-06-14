using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Runtime.Types;
using Annium.Logging;
using Annium.Reflection;

namespace Annium.Core.Runtime.Internal.Types;

/// <summary>
/// Internal implementation of type resolver that handles generic type resolution
/// </summary>
internal class TypeResolver : ITypeResolver, ILogSubject
{
    /// <summary>
    /// The logger for this type resolver
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The type manager used for type lookup
    /// </summary>
    private readonly ITypeManager _typeManager;

    /// <summary>
    /// Initializes a new instance of TypeResolver with the specified type manager and logger
    /// </summary>
    /// <param name="typeManager">The type manager to use for type resolution</param>
    /// <param name="logger">The logger to use for tracing operations</param>
    public TypeResolver(ITypeManager typeManager, ILogger logger)
    {
        Logger = logger;
        _typeManager = typeManager;
    }

    /// <summary>
    /// Resolves a collection of types based on the specified type criteria
    /// </summary>
    /// <param name="type">The type to use as resolution criteria</param>
    /// <returns>A collection of resolved types</returns>
    public IReadOnlyCollection<Type> ResolveType(Type type)
    {
        if (!type.IsGenericType)
            return new[] { type };

        this.Trace<string>("{type} - start", type.FriendlyName());
        var sets = new List<Type[]>();
        foreach (var argument in type.GetGenericArguments())
        {
            if (argument.GetGenericParameterConstraints().Length == 0)
                throw new ArgumentException(
                    $"Can't use generic Profile {type} with unconstrained parameter {argument}"
                );

            var implementations = _typeManager
                .Types.Where(x => !x.ContainsGenericParameters)
                .Select(x => x.GetTargetImplementation(argument))
                .Where(x => x != null)
                .ToArray();
            sets.Add(implementations!);
        }

        var combinations = GetCombinations(sets).ToArray();
        var types = combinations.Select(type.MakeGenericType).ToArray();

        this.Trace<string>("{type} - end", type.FriendlyName());

        return types;
    }

    /// <summary>
    /// Generates all possible combinations from the given sets of types
    /// </summary>
    /// <param name="sets">The sets of types to generate combinations from</param>
    /// <returns>All possible combinations of types</returns>
    private IReadOnlyCollection<Type[]> GetCombinations(IReadOnlyCollection<Type[]> sets)
    {
        if (sets.Count == 1)
            return sets.ElementAt(0).Select(x => new[] { x }).ToArray();

        var result = sets.ElementAt(0)
            .SelectMany(x => GetCombinations(sets.Skip(1).ToArray()).Select(y => y.Prepend(x)))
            .Select(x => x.ToArray())
            .ToArray();

        return result;
    }
}
