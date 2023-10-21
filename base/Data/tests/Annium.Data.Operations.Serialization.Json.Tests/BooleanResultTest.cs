using System;
using System.IO;
using Annium.Core.DependencyInjection;
using Annium.Data.Operations.Serialization.Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Data.Operations.Serialization.Json.Tests;

public class BooleanResultTest : BooleanResultTestBase
{
    public BooleanResultTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(container => container.AddSerializers().WithJson(opts => opts.ConfigureForOperations(), true));
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(byte[]))]
    [InlineData(typeof(ReadOnlyMemory<byte>))]
    [InlineData(typeof(Stream))]
    public void Simple(Type type)
    {
        GetType().GetMethod(nameof(Simple_Base))!.MakeGenericMethod(type).Invoke(this, Array.Empty<object>());
    }

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