using System;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

[AttributeUsage(AttributeTargets.Method)]
public class BeforeAttribute : Attribute, ILocatedAttribute
{
    public string File { get; }

    public int Line { get; }

    public string SetUpName { get; }

    public BeforeAttribute(string setUpName, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        File = file;
        Line = line;
        SetUpName = setUpName;
    }
}