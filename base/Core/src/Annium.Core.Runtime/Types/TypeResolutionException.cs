using System;

namespace Annium.Core.Runtime.Types;

public class TypeResolutionException : Exception
{
    public TypeResolutionException(Type src, Type tgt, params string[] messages) :
        base($"Can't convert {src.FullName} -> {tgt.FullName}. {string.Join(Environment.NewLine, messages)}")
    {
    }
}