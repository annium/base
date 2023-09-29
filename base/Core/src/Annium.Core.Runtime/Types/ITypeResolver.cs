using System;
using System.Collections.Generic;

namespace Annium.Core.Runtime.Types;

public interface ITypeResolver
{
    IReadOnlyCollection<Type> ResolveType(Type type);
}