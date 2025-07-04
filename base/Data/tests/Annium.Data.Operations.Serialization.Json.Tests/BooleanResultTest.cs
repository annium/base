using System;
using System.IO;
using Annium.Data.Operations.Serialization.Tests.Base;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Xunit;

namespace Annium.Data.Operations.Serialization.Json.Tests;

/// <summary>
/// Tests for JSON serialization of BooleanResult types.
/// </summary>
public class BooleanResultTest : BooleanResultTestBase
{
    /// <summary>
    /// Initializes a new instance of the BooleanResultTest class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public BooleanResultTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
            container
                .AddSerializers()
                .WithJson(opts => JsonSerializerOptionsExtensions.ConfigureForOperations(opts), true)
        );
    }

    /// <summary>
    /// Tests that simple BooleanResult types can be serialized and deserialized correctly using JSON.
    /// </summary>
    /// <param name="type">The type to test serialization for.</param>
    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(byte[]))]
    [InlineData(typeof(ReadOnlyMemory<byte>))]
    [InlineData(typeof(Stream))]
    public void Simple(Type type)
    {
        GetType().GetMethod(nameof(Simple_Base))!.MakeGenericMethod(type).Invoke(this, Array.Empty<object>());
    }

    /// <summary>
    /// Tests that BooleanResult types with data can be serialized and deserialized correctly using JSON.
    /// </summary>
    /// <param name="type">The type to test serialization for.</param>
    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(byte[]))]
    [InlineData(typeof(ReadOnlyMemory<byte>))]
    [InlineData(typeof(Stream))]
    public void Data(Type type)
    {
        GetType().GetMethod(nameof(Data_Base))!.MakeGenericMethod(type).Invoke(this, Array.Empty<object>());
    }
}
