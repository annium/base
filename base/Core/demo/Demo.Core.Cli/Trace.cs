using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Annium.Reflection;

namespace Demo.Core.Cli;

public static class Trace
{
    public static void App()
    {
        Domain();
        Contexts();
    }

    private static void Domain()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        Console.WriteLine($"AppDomain has ({assemblies.Length}) assemblies:");
        foreach (var assembly in assemblies)
            Console.WriteLine($"   - {assembly.FriendlyName()} at {AssemblyLoadContext.GetLoadContext(assembly)}");
    }

    private static void Contexts()
    {
        var contexts = AssemblyLoadContext.All.ToArray();
        Console.WriteLine($"App has registered {contexts.Length} contexts:");
        foreach (var context in contexts)
            Context(context);
    }

    private static void Context(AssemblyLoadContext context)
    {
        var assemblies = context.Assemblies.ToArray();
        Console.WriteLine($"  Context {context} with {assemblies.Length} assemblies:");
        foreach (var assembly in assemblies)
            Console.WriteLine($"   - {assembly.FriendlyName()}");
    }

    public static void Assemblies(Assembly[] assemblies)
    {
        Console.WriteLine($"Assemblies ({assemblies.Length}):");
        foreach (var assembly in assemblies)
            TraceAssembly(assembly);
    }

    private static void TraceAssembly(Assembly assembly)
    {
        var context = AssemblyLoadContext.GetLoadContext(assembly);
        var dependencies = assembly.GetReferencedAssemblies();
        Console.WriteLine($"- {assembly.FriendlyName()} is resided at {context} and has ({dependencies.Length}) dependencies:");
        foreach (var dependency in dependencies)
            Console.WriteLine($"  - {dependency}");
    }
}