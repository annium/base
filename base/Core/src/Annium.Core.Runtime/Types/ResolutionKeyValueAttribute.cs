using System;

namespace Annium.Core.Runtime.Types;

[AttributeUsage(AttributeTargets.Class)]
public class ResolutionKeyValueAttribute : Attribute
{
    public object Key { get; }

    public ResolutionKeyValueAttribute(object key)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
    }
}