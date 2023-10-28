using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack.Tests;

public class TestBase
{
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
