using System;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SkipAttribute : Attribute, ILocatedAttribute
{
    public string File { get; }

    public int Line { get; }

    public SkipAttribute([CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        File = file;
        Line = line;
    }
}