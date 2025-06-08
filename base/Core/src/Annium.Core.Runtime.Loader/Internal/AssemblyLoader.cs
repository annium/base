using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Annium.Core.Runtime.Loader.Internal;

/// <summary>
/// Internal implementation of assembly loader that uses custom resolution logic
/// </summary>
internal class AssemblyLoader : IAssemblyLoader
{
    /// <summary>
    /// The assembly load context used for loading assemblies
    /// </summary>
    private readonly AssemblyLoadContext _context;

    /// <summary>
    /// Initializes a new instance of AssemblyLoader with specified resolvers
    /// </summary>
    /// <param name="pathResolvers">Collection of functions that resolve assembly names to file paths</param>
    /// <param name="byteArrayResolvers">Collection of functions that resolve assembly names to byte arrays</param>
    public AssemblyLoader(
        IReadOnlyCollection<Func<AssemblyName, string?>> pathResolvers,
        IReadOnlyCollection<Func<AssemblyName, Task<byte[]>?>> byteArrayResolvers
    )
    {
        _context = new ResolvingLoadContext(pathResolvers, byteArrayResolvers);
    }

    /// <summary>
    /// Loads an assembly by name and all its dependencies
    /// </summary>
    /// <param name="name">The name of the assembly to load</param>
    /// <returns>The loaded assembly</returns>
    public Assembly Load(string name)
    {
        var registry = new Dictionary<string, Assembly>();
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            if (!registry.ContainsKey(asm.GetName().FullName))
                registry[asm.GetName().FullName] = asm;

        var asmName = new AssemblyName(name);
        Load(
            asmName,
            assemblyName => registry.ContainsKey(assemblyName.FullName),
            assemblyName => registry[assemblyName.FullName] = default!,
            (assemblyName, assembly) => registry[assemblyName.FullName] = assembly
        );
        var result = registry[asmName.FullName];

        return result;
    }

    /// <summary>
    /// Recursively loads an assembly and its dependencies
    /// </summary>
    /// <param name="name">The assembly name to load</param>
    /// <param name="isRegistered">Function to check if assembly is already registered</param>
    /// <param name="lockRegistration">Action to lock registration for the assembly</param>
    /// <param name="register">Action to register the loaded assembly</param>
    private void Load(
        AssemblyName name,
        Func<AssemblyName, bool> isRegistered,
        Action<AssemblyName> lockRegistration,
        Action<AssemblyName, Assembly> register
    )
    {
        if (isRegistered(name))
            return;

        lockRegistration(name);
        Assembly assembly;
        try
        {
            assembly = _context.LoadFromAssemblyName(name);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to load {name}: {e}");
            return;
        }

        register(name, assembly);

        foreach (var referenceName in assembly.GetReferencedAssemblies())
            Load(referenceName, isRegistered, lockRegistration, register);
    }
}
