using System;

// ReSharper disable once CheckNamespace
namespace Annium.Extensions.Arguments;

[AttributeUsage(AttributeTargets.Property)]
public class HelpAttribute : BaseAttribute
{
    public string Help { get; }

    public HelpAttribute(string help)
    {
        Help = help;
    }
}