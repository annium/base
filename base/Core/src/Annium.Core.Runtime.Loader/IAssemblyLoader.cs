using System.Reflection;

namespace Annium.Core.Runtime.Loader;

/// <summary>
/// Interface for loading assemblies with custom resolution logic
/// </summary>
public interface IAssemblyLoader
{
    /// <summary>
    /// Loads an assembly by name using configured resolvers
    /// </summary>
    /// <param name="name">The name of the assembly to load</param>
    /// <returns>The loaded assembly</returns>
    Assembly Load(string name);
}
