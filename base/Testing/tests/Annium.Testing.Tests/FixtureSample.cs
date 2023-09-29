using System;

namespace Annium.Testing.Tests;

[Fixture]
public class FixtureSample : IDisposable
{
    public FixtureSample()
    {
        Console.WriteLine("Create FixtureSample");
    }

    public void Dispose()
    {
        Console.WriteLine("Dispose FixtureSample");
    }
}