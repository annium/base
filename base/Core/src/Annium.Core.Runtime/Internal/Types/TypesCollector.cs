using System;
using System.Collections.Generic;
using System.Reflection;
using Annium.Logging;
using Annium.Reflection;

namespace Annium.Core.Runtime.Internal.Types;

/// <summary>
/// Internal collector that gathers all types from a collection of assemblies
/// </summary>
internal class TypesCollector : ILogSubject
{
    /// <summary>
    /// The logger for this types collector
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of TypesCollector with the specified logger
    /// </summary>
    /// <param name="logger">The logger to use for tracing collection operations</param>
    public TypesCollector(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Collects all types from the specified assemblies
    /// </summary>
    /// <param name="assemblies">The assemblies to collect types from</param>
    /// <returns>Collection of all types found in the assemblies</returns>
    public IReadOnlyCollection<Type> Collect(IReadOnlyCollection<Assembly> assemblies)
    {
        this.Trace("start");

        // list of collected types
        var types = new HashSet<Type>();

        foreach (var assembly in assemblies)
        {
            var assemblyTypes = assembly.GetTypes();
            this.Trace<int, string>(
                "register {assemblyTypesLength} type(s) from assembly {assembly}",
                assemblyTypes.Length,
                assembly.ShortName()
            );
            foreach (var type in assemblyTypes)
                types.Add(type);
        }

        this.Trace("done");

        return types;
    }
}
