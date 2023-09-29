using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.MessagePack.Tests;

public class TestBase
{
    protected ISerializer<ReadOnlyMemory<byte>> GetSerializer()
    {
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddSerializers().WithMessagePack();
        container.AddTime().WithManagedTime().SetDefault();
        container.AddLogging();

        var sp = container.BuildServiceProvider();
        sp.UseLogging(x => x.UseInMemory());

        var serializer = sp.ResolveSerializer<ReadOnlyMemory<byte>>(Abstractions.Constants.DefaultKey, Constants.MediaType);

        return serializer;
    }
}