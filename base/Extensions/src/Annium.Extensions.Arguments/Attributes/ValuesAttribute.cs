using System;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

[AttributeUsage(AttributeTargets.Property)]
public class ValuesAttribute : BaseAttribute
{
    public string[] Values { get; }

    public ValuesAttribute(params string[] values)
    {
        Values = values;
    }
}