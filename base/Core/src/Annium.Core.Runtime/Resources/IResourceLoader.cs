using System.Collections.Generic;
using System.Reflection;

namespace Annium.Core.Runtime.Resources;

public interface IResourceLoader
{
    IReadOnlyCollection<IResource> Load(string prefix);

    IReadOnlyCollection<IResource> Load(string prefix, Assembly assembly);
}