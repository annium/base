using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Logging;
using Annium.Reflection;

namespace Annium.Core.Runtime.Internal.Types;

/// <summary>
/// Internal collector that gathers assemblies for type scanning based on AutoScannedAttribute
/// </summary>
internal class AssembliesCollector : ILogSubject
{
    /// <summary>
    /// The type ID for AutoScannedAttribute used for efficient comparison
    /// </summary>
    private static readonly TypeId _autoScannedTypeId = typeof(AutoScannedAttribute).GetTypeId();

    /// <summary>
    /// The logger for this collector
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of AssembliesCollector with the specified logger
    /// </summary>
    /// <param name="logger">The logger to use for tracing collection operations</param>
    public AssembliesCollector(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Collects all assemblies that should be included in type scanning starting from the specified assembly
    /// </summary>
    /// <param name="assembly">The starting assembly to collect dependencies from</param>
    /// <returns>Collection of assemblies marked for auto-scanning</returns>
    public IReadOnlyCollection<Assembly> Collect(Assembly assembly)
    {
        this.Trace("start");

        // result parts
        var allAssemblies = new Dictionary<string, Assembly>();
        var processedAssemblies = new HashSet<Assembly>();
        var matchedAssemblies = new HashSet<Assembly>();

        // collect assemblies, already residing in AppDomain
        this.Trace("register AppDomain assemblies");
        foreach (var domainAssembly in AppDomain.CurrentDomain.GetAssemblies())
            if (domainAssembly.FullName != null! && !allAssemblies.ContainsKey(domainAssembly.FullName))
            {
                this.Trace<string, string>(
                    "{domainAssemblyName} - register with {domainAssemblyFullName}",
                    domainAssembly.FriendlyName(),
                    domainAssembly.FullName
                );
                allAssemblies[domainAssembly.FullName] = domainAssembly;
            }

        var resolveAssembly = LoadAssembly(allAssemblies);

        this.Trace("collect {assembly} dependencies", assembly);
        Collect(assembly.GetName(), resolveAssembly, processedAssemblies.Add, asm => matchedAssemblies.Add(asm));

        this.Trace("done");

        return matchedAssemblies;
    }

    /// <summary>
    /// Recursively collects assemblies and their dependencies for type scanning
    /// </summary>
    /// <param name="name">The assembly name to collect</param>
    /// <param name="resolveAssembly">Function to resolve assembly by name</param>
    /// <param name="registerAssembly">Function to register an assembly as processed</param>
    /// <param name="addMatchedAssembly">Action to add a matched assembly to the collection</param>
    private void Collect(
        AssemblyName name,
        Func<AssemblyName, Assembly?> resolveAssembly,
        Func<Assembly, bool> registerAssembly,
        Action<Assembly> addMatchedAssembly
    )
    {
        var assembly = resolveAssembly(name);
        if (assembly is null)
        {
            this.Trace<string?>("{name} - not resolved", name.Name);
            return;
        }

        if (!registerAssembly(assembly))
            return;

        var autoScanned = assembly
            .GetCustomAttributes()
            .SingleOrDefault(x => x.GetType().GetTypeId() == _autoScannedTypeId);
        if (autoScanned is null)
            this.Trace<string?>("{name} - not marked as auto-scanned", name.Name);
        else
        {
            this.Trace<string?>("{name} - matched", name.Name);
            addMatchedAssembly(assembly);
            var dependencies = (Assembly[])
                autoScanned.GetType().GetProperty(nameof(AutoScannedAttribute.Dependencies))!.GetValue(autoScanned)!;
            foreach (var dependency in dependencies)
            {
                this.Trace<string?, string>("{name} - add dependency {dependency}", name.Name, dependency.ShortName());
                addMatchedAssembly(dependency);
                Collect(dependency.GetName(), resolveAssembly, registerAssembly, addMatchedAssembly);
            }
        }

        foreach (var assemblyName in assembly.GetReferencedAssemblies())
            Collect(assemblyName, resolveAssembly, registerAssembly, addMatchedAssembly);
    }

    /// <summary>
    /// Creates a function for loading assemblies with caching
    /// </summary>
    /// <param name="assemblies">Dictionary to cache loaded assemblies</param>
    /// <returns>Function that loads and caches assemblies by name</returns>
    private Func<AssemblyName, Assembly?> LoadAssembly(IDictionary<string, Assembly> assemblies) =>
        name =>
        {
            if (assemblies.TryGetValue(name.FullName, out var asm))
                return asm;

            this.Trace("load {name}", name);
            return assemblies[name.FullName] = AppDomain.CurrentDomain.Load(name);
        };
}
