using System;
using System.Collections.Generic;
using System.Reflection;
using Annium.Logging;
using Annium.Reflection;

namespace Annium.Core.Runtime.Internal.Types;

internal class TypesCollector : ILogSubject
{
    public ILogger Logger { get; }

    public TypesCollector(ILogger logger)
    {
        Logger = logger;
    }

    public IReadOnlyCollection<Type> Collect(IReadOnlyCollection<Assembly> assemblies)
    {
        this.Trace("start");

        // list of collected types
        var types = new HashSet<Type>();

        foreach (var assembly in assemblies)
        {
            var assemblyTypes = assembly.GetTypes();
            this.Trace<int, string>("register {assemblyTypesLength} type(s) from assembly {assembly}", assemblyTypes.Length, assembly.ShortName());
            foreach (var type in assemblyTypes)
                types.Add(type);
        }

        this.Trace("done");

        return types;
    }
}