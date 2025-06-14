using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.BinaryString.Tests;

/// <summary>
/// Base class for binary string serialization tests providing common setup
/// </summary>
public class TestBase
{
    /// <summary>
    /// Gets a configured binary string serializer for testing
    /// </summary>
    /// <returns>A serializer that converts byte arrays to hex strings</returns>
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
