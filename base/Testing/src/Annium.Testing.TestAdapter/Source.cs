using System.Reflection;
using System.Runtime.Loader;

namespace Annium.Testing.TestAdapter;

internal static class Source
{
    public static Assembly Resolve(string name) =>
        AssemblyLoadContext.Default.LoadFromAssemblyName(AssemblyLoadContext.GetAssemblyName(name));
}