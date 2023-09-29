using System.Reflection;

namespace Annium.Core.Runtime.Loader;

public interface IAssemblyLoader
{
    Assembly Load(string name);
}