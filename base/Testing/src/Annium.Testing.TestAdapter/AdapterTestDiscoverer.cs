using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Annium.Testing.TestAdapter;

[FileExtension(Constants.FileExtensionDll)]
[FileExtension(Constants.FileExtensionExe)]
[DefaultExecutorUri(Constants.ExecutorUri)]
public class AdapterTestDiscoverer : ITestDiscoverer, ILogSubject
{
    public ILogger Logger { get; private set; } = default!;
    private readonly TestConverter _testConverter;
    private TestDiscoverer? _testDiscoverer;

    public AdapterTestDiscoverer()
    {
        var factory = new ServiceProviderFactory();
        var provider = factory.CreateServiceProvider(factory.CreateBuilder(new ServiceCollection()).UseServicePack<ServicePack>());

        _testConverter = provider.Resolve<TestConverter>();
    }

    public void DiscoverTests(
        IEnumerable<string> sources,
        IDiscoveryContext discoveryContext,
        IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink
    )
    {
        var provider = AdapterServiceProviderBuilder.Build(discoveryContext);
        _testDiscoverer = provider.Resolve<TestDiscoverer>();
        Logger = provider.Resolve<ILogger>();

        this.Debug("Start discovery.");

        DiscoverSourcesAsync(sources, discoverySink).Wait();
    }

    private Task DiscoverSourcesAsync(IEnumerable<string> sources, ITestCaseDiscoverySink discoverySink) =>
        Task.WhenAll(sources.Select(source => DiscoverAssemblyTestsAsync(source, discoverySink)));

    private Task DiscoverAssemblyTestsAsync(string source, ITestCaseDiscoverySink discoverySink)
    {
        var assembly = Source.Resolve(source);

        this.Debug<string?>("Start discovery of {assembly}.", assembly.FullName);

        return _testDiscoverer!.FindTestsAsync(
            assembly,
            test => discoverySink.SendTestCase(_testConverter.Convert(assembly, test))
        );
    }
}