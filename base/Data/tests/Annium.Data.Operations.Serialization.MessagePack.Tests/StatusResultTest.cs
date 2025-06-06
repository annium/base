using System;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations.Serialization.Tests.Base;
using MessagePack;
using MessagePack.Resolvers;
using Xunit;

namespace Annium.Data.Operations.Serialization.MessagePack.Tests;

public class StatusResultTest : StatusResultTestBase
{
    public StatusResultTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
            container
                .AddSerializers()
                .WithMessagePack(
                    () =>
                        new MessagePackSerializerOptions(
                            CompositeResolver.Create(Resolver.Instance, MessagePackSerializerOptions.Standard.Resolver)
                        ),
                    true
                )
        );
    }

    [Theory]
    [InlineData(typeof(ReadOnlyMemory<byte>))]
    public void Simple(Type type)
    {
        GetType().GetMethod(nameof(Simple_Base))!.MakeGenericMethod(type).Invoke(this, Array.Empty<object>());
    }

    [Theory]
    [InlineData(typeof(ReadOnlyMemory<byte>))]
    public void Data(Type type)
    {
        GetType().GetMethod(nameof(Data_Base))!.MakeGenericMethod(type).Invoke(this, Array.Empty<object>());
    }
}
