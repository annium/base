using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Serialization.Json.Tests;

public class ResultConverterTest
{
    [Fact]
    public void BaseWrite_Base_WritesCorrectly()
    {
        // act
        var str = JsonSerializer.Serialize(Result.New(), GetSettings());

        // assert
        str.Is(@"{""plainErrors"":[],""labeledErrors"":{}}");
    }

    [Fact]
    public void BaseWrite_WithErrors_WritesCorrectly()
    {
        // act
        var str = JsonSerializer.Serialize(Result.New().Error("plain").Error("label", "another"), GetSettings());

        // assert
        str.Is(@"{""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}");
    }

    [Fact]
    public void DataWrite_Base_WritesCorrectly()
    {
        // act
        var str = JsonSerializer.Serialize(Result.New(5), GetSettings());

        // assert
        str.Is(@"{""data"":5,""plainErrors"":[],""labeledErrors"":{}}");
    }

    [Fact]
    public void DataWrite_WithErrors_WritesCorrectly()
    {
        // act
        var str = JsonSerializer.Serialize(Result.New(5).Error("plain").Error("label", "another"), GetSettings());

        // assert
        str.Is(@"{""data"":5,""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}");
    }

    [Fact]
    public void BaseRead_Blank_ReadsCorrectly()
    {
        // act
        var result = JsonSerializer.Deserialize<IResult>("{}", GetSettings())!;

        // assert
        result.IsOk.IsTrue();
    }

    [Fact]
    public void BaseRead_WithErrors_ReadsCorrectly()
    {
        // act
        var result = JsonSerializer.Deserialize<IResult>(@"{""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}", GetSettings())!;

        // assert
        result.HasErrors.IsTrue();
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is("plain");
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At("label").At(0).Is("another");
    }

    [Fact]
    public void DataRead_Blank_ReadsCorrectly()
    {
        // act
        var result = JsonSerializer.Deserialize<IResult<int>>(@"{""data"":5}", GetSettings())!;

        // assert
        result.IsOk.IsTrue();
        result.Data.Is(5);
    }

    [Fact]
    public void DataRead_WithErrors_ReadsCorrectly()
    {
        // act
        var result = JsonSerializer.Deserialize<IResult<int>>(@"{""data"":5,""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}",
            GetSettings())!;

        // assert
        result.HasErrors.IsTrue();
        result.Data.Is(5);
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is("plain");
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At("label").At(0).Is("another");
    }

    private JsonSerializerOptions GetSettings() => new JsonSerializerOptions().ConfigureForOperations();
}