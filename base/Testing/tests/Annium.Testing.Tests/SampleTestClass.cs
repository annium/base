using System;
using System.Threading.Tasks;

namespace Annium.Testing.Tests;

public class SampleTestClass : IDisposable
{
    // ReSharper disable once UnusedParameter.Local
    public SampleTestClass(FixtureSample fixture)
    {
        Console.WriteLine("Create SampleTestClass");
    }

    [Fact]
    public void Test_Succeed()
    {
        Console.WriteLine(nameof(Test_Succeed));
        true.IsTrue();
    }

    [Fact]
    public void Test_Failing()
    {
        Console.WriteLine(nameof(Test_Failing));
        Wrap.It((Action)(() => throw new ArgumentException())).Throws<ArgumentException>();
    }

    [Fact]
    [Skip]
    public void Test_Skipped()
    {
        Console.WriteLine(nameof(Test_Skipped));
    }

    [Fact]
    [Before(nameof(Before))]
    [After(nameof(After))]
    public async Task Test_Async()
    {
        Console.WriteLine(nameof(Test_Async));
        await Task.CompletedTask;
        true.IsTrue();
    }

    public void Before()
    {
        Console.WriteLine("Before");
    }

    public void After()
    {
        Console.WriteLine("After");
    }

    public void Dispose()
    {
        Console.WriteLine("Dispose SampleTestClass");
    }
}