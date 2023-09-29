using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing.Elements;

namespace Annium.Testing;

public class TestDiscoverer : ILogSubject
{
    public ILogger Logger { get; }

    public TestDiscoverer(ILogger logger)
    {
        Logger = logger;
    }

    public Task FindTestsAsync(
        Assembly assembly,
        Action<Test> handleFound
    )
    {
        assembly.GetTypes()
            .AsParallel()
            .WithDegreeOfParallelism(Environment.ProcessorCount)
            .ForAll(type => FindTestClassTests(type, handleFound));

        return Task.CompletedTask;
    }

    private void FindTestClassTests(Type testClass, Action<Test> handleTestFound)
    {
        if (testClass.GetCustomAttribute<SkipAttribute>() != null)
        {
            this.Debug<string?>("{testClass} is skipped", testClass.FullName);
            return;
        }

        this.Trace<string?>("search in {testClass}", testClass.FullName);
        foreach (var test in testClass.GetMethods().Where(IsTest).Select(method => new Test(method)))
            handleTestFound(test);
    }

    private bool IsTest(MethodInfo candidate) =>
        candidate.GetCustomAttribute<FactAttribute>() != null &&
        candidate.GetCustomAttribute<SkipAttribute>() == null &&
        !candidate.IsGenericMethod &&
        candidate.GetParameters().Length == 0;
}