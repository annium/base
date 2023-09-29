using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Testing.Elements;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Annium.Testing.TestAdapter;

[ExtensionUri(Constants.ExecutorUri)]
public class AdapterTestExecutor : ITestExecutor, ILogSubject
{
    public ILogger Logger { get; private set; } = default!;
    private readonly TestConverter _testConverter;
    private readonly TestResultConverter _testResultConverter;
    private IServiceProvider? _provider;

    public AdapterTestExecutor()
    {
        var factory = new ServiceProviderFactory();
        var provider = factory.CreateServiceProvider(factory.CreateBuilder(new ServiceCollection()).UseServicePack<ServicePack>());
        _testConverter = provider.Resolve<TestConverter>();
        _testResultConverter = provider.Resolve<TestResultConverter>();
    }

    public void RunTests(
        IEnumerable<string>? sources,
        IRunContext? runContext,
        IFrameworkHandle? frameworkHandle
    )
    {
        if (sources == null) throw new ArgumentNullException(nameof(sources));
        if (runContext == null) throw new ArgumentNullException(nameof(runContext));
        if (frameworkHandle == null) throw new ArgumentNullException(nameof(frameworkHandle));

        if (runContext.IsBeingDebugged)
            Debugger.Launch();

        _provider = AdapterServiceProviderBuilder.Build(runContext);
        Logger = _provider.Resolve<ILogger>();

        this.Debug("Start execution.");

        Task.WhenAll(sources.Select(source => RunAssemblyTestsAsync(Source.Resolve(source), frameworkHandle))).Wait();
    }

    public void RunTests(
        IEnumerable<TestCase>? tests,
        IRunContext? runContext,
        IFrameworkHandle? frameworkHandle
    )
    {
        if (tests == null) throw new ArgumentNullException(nameof(tests));
        if (runContext == null) throw new ArgumentNullException(nameof(runContext));
        if (frameworkHandle == null) throw new ArgumentNullException(nameof(frameworkHandle));
        var testSet = tests.ToArray();

        if (runContext.IsBeingDebugged)
            Debugger.Launch();

        _provider = AdapterServiceProviderBuilder.Build(runContext);
        Logger = _provider.Resolve<ILogger>();

        this.Debug("Start execution.");

        Task.WhenAll(testSet.Select(test => test.Source).Distinct().Select(
                source => RunAssemblyTestsAsync(Source.Resolve(source), testSet.Where(test => test.Source == source), frameworkHandle)
            )
        );
    }

    public void Cancel()
    {
    }

    private async Task RunAssemblyTestsAsync(Assembly assembly, IFrameworkHandle frameworkHandle)
    {
        this.Debug<string?>("Start execution of all tests in {assembly}.", assembly.FullName);

        var tests = new List<Test>();
        await _provider!.Resolve<TestDiscoverer>().FindTestsAsync(assembly, tests.Add);

        await RunTestsAsync(assembly, tests, frameworkHandle);
    }

    private Task RunAssemblyTestsAsync(
        Assembly assembly,
        IEnumerable<TestCase> testCases,
        IFrameworkHandle frameworkHandle
    )
    {
        var testCasesSet = testCases.ToArray();

        this.Debug<int, string?>("Start execution of specific {testCasesCount} tests in {assembly}.", testCasesSet.Length, assembly.FullName);

        var tests = testCasesSet.Select(testCase => _testConverter.Convert(assembly, testCase)).ToArray();

        return RunTestsAsync(assembly, tests, frameworkHandle);
    }

    private Task RunTestsAsync(
        Assembly assembly,
        IEnumerable<Test> tests,
        IFrameworkHandle frameworkHandle
    )
    {
        var cfg = _provider!.Resolve<TestingConfiguration>();
        var testSet = tests.FilterMask(cfg.Filter).ToArray();

        return GetExecutor(assembly, testSet)
            .RunTestsAsync(
                testSet,
                (test, result) => frameworkHandle.RecordResult(_testResultConverter.Convert(assembly, test, result))
            );
    }

    private TestExecutor GetExecutor(Assembly assembly, IEnumerable<Test> tests)
    {
        var testSet = tests.ToArray();

        this.Trace("Build test executor for assembly {assembly} and given {testsCount} tests.", assembly.FullName, testSet.Length);

        var container = AssemblyServicesCollector.Collect(assembly, testSet);
        container.Add(_provider!.Resolve<TestingConfiguration>()).AsSelf().Singleton();

        var factory = new ServiceProviderFactory();
        var provider = factory.CreateServiceProvider(factory.CreateBuilder(container.Collection).UseServicePack<Testing.ServicePack>());

        return provider.Resolve<TestExecutor>();
    }
}