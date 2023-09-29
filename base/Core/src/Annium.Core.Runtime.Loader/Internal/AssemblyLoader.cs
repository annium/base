using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Annium.Core.Runtime.Loader.Internal;

internal class AssemblyLoader : IAssemblyLoader
{
    private readonly AssemblyLoadContext _context;

    public AssemblyLoader(
        IReadOnlyCollection<Func<AssemblyName, string?>> pathResolvers,
        IReadOnlyCollection<Func<AssemblyName, Task<byte[]>?>> byteArrayResolvers
    )
    {
        _context = new ResolvingLoadContext(pathResolvers, byteArrayResolvers);
    }

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