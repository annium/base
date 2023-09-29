using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.BinaryString.Tests;

public class TestBase
{
    protected ISerializer<byte[], string> GetSerializer()
    {
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddSerializers().WithBinaryString();

        var sp = container.BuildServiceProvider();
        var serializer = sp.ResolveSerializer<byte[], string>(Abstractions.Constants.DefaultKey, Constants.MediaType);

        return serializer;
    }
}