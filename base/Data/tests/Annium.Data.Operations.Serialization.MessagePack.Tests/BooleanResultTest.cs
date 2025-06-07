using System;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations.Serialization.Tests.Base;
using MessagePack;
using MessagePack.Resolvers;
using Xunit;

namespace Annium.Data.Operations.Serialization.MessagePack.Tests;

/// <summary>
/// Tests for MessagePack serialization of BooleanResult types.
/// </summary>
public class BooleanResultTest : BooleanResultTestBase
{
    public BooleanResultTest(ITestOutputHelper outputHelper)
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

    /// <summary>
    /// Tests that simple BooleanResult types can be serialized and deserialized correctly using MessagePack.
    /// </summary>
    /// <param name="type">The type to test serialization for.</param>
    [Theory]
    [InlineData(typeof(ReadOnlyMemory<byte>))]
    public void Simple(Type type)
    {
        GetType().GetMethod(nameof(Simple_Base))!.MakeGenericMethod(type).Invoke(this, Array.Empty<object>());
    }

    /// <summary>
    /// Tests that BooleanResult types with data can be serialized and deserialized correctly using MessagePack.
    /// </summary>
    /// <param name="type">The type to test serialization for.</param>
    [Theory]
    [InlineData(typeof(ReadOnlyMemory<byte>))]
    public void Data(Type type)
    {
        GetType().GetMethod(nameof(Data_Base))!.MakeGenericMethod(type).Invoke(this, Array.Empty<object>());
    }
}
