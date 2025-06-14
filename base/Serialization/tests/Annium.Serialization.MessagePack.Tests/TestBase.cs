using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack.Tests;

/// <summary>
/// Base class for MessagePack serialization tests providing common setup
/// </summary>
public class TestBase
{
    /// <summary>
    /// Gets a configured MessagePack serializer for testing
    /// </summary>
    /// <param name="configure">Optional configuration function for MessagePack options</param>
    /// <returns>A MessagePack serializer with the specified configuration</returns>
    protected ISerializer<ReadOnlyMemory<byte>> GetSerializer(Func<MessagePackSerializerOptions>? configure = null)
    {
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddSerializers().WithMessagePack(configure ?? (() => MessagePackSerializerOptions.Standard));
        container.AddTime().WithManagedTime().SetDefault();
        container.AddLogging();

        var sp = container.BuildServiceProvider();
        sp.UseLogging(x => x.UseInMemory());

        var serializer = sp.ResolveSerializer<ReadOnlyMemory<byte>>(
            Abstractions.Constants.DefaultKey,
            Constants.MediaType
        );

        return serializer;
    }
}
