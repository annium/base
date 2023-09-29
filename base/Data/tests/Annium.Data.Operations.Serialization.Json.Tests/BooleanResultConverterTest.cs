using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Serialization.Json.Tests;

public class BooleanResultConverterTest
{
    [Fact]
    public void BaseWrite_Blank_WritesCorrectly()
    {
        // act
        var str = JsonSerializer.Serialize(Result.Success(), GetSettings());

        // assert
        str.Is(@"{""isSuccess"":true,""isFailure"":false,""plainErrors"":[],""labeledErrors"":{}}");
    }

    [Fact]
    public void BaseWrite_WithErrors_WritesCorrectly()
    {
        // act
        var str = JsonSerializer.Serialize(Result.Success().Error("plain").Error("label", "another"), GetSettings());

        // assert
        str.Is(@"{""isSuccess"":true,""isFailure"":false,""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}");
    }

    [Fact]
    public void BaseRead_Blank_ReadsCorrectly()
    {
        // act
        var result = JsonSerializer.Deserialize<IBooleanResult>("{}", GetSettings())!;

        // assert
        result.IsSuccess.IsFalse();
        result.IsFailure.IsTrue();
    }

    [Fact]
    public void BaseRead_Result_ReadsCorrectly()
    {
        // act
        var result = JsonSerializer.Deserialize<IBooleanResult>(@"{""isSuccess"":true}", GetSettings())!;

        // assert
        result.IsSuccess.IsTrue();
        result.IsFailure.IsFalse();
    }

    [Fact]
    public void BaseRead_WithErrors_ReadsCorrectly()
    {
        // act
        var result = JsonSerializer.Deserialize<IBooleanResult>(
            @"{""isSuccess"":true,""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}"
            , GetSettings()
        )!;

        // assert
        result.IsSuccess.IsTrue();
        result.IsFailure.IsFalse();
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is("plain");
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At("label").At(0).Is("another");
    }

    [Fact]
    public void DataWrite_Blank_WritesCorrectly()
    {
        // act
        var str = JsonSerializer.Serialize(Result.Success(5), GetSettings());

        // assert
        str.Is(@"{""isSuccess"":true,""isFailure"":false,""data"":5,""plainErrors"":[],""labeledErrors"":{}}");
    }

    [Fact]
    public void DataWrite_WithErrors_WritesCorrectly()
    {
        // act
        var str = JsonSerializer.Serialize(Result.Success(5).Error("plain").Error("label", "another"), GetSettings());

        // assert
        str.Is(@"{""isSuccess"":true,""isFailure"":false,""data"":5,""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}");
    }

    [Fact]
    public void DataRead_BlankValueType_ReadsCorrectly()
    {
        // act
        var result = JsonSerializer.Deserialize<IBooleanResult<int>>("{}", GetSettings())!;

        // assert
        result.Data.Is(0);
        result.IsSuccess.IsFalse();
        result.IsFailure.IsTrue();
    }

    [Fact]
    public void DataRead_BlankReferenceType_ReadsCorrectly()
    {
        // act
        var result = JsonSerializer.Deserialize<IBooleanResult<string>>("{}", GetSettings())!;

        // assert
        result.Data.IsDefault();
        result.IsSuccess.IsFalse();
        result.IsFailure.IsTrue();
    }

    [Fact]
    public void DataRead_Result_ReadsCorrectly()
    {
        // act
        var result = JsonSerializer.Deserialize<IBooleanResult<int>>(@"{""data"":5,""isSuccess"":true}", GetSettings())!;

        // assert
        result.Data.Is(5);
        result.IsSuccess.IsTrue();
        result.IsFailure.IsFalse();
    }

    [Fact]
    public void DataRead_WithErrors_ReadsCorrectly()
    {
        // act
        var result = JsonSerializer.Deserialize<IBooleanResult<int>>(
            @"{""data"":5,""isSuccess"":true,""plainErrors"":[""plain""],""labeledErrors"":{""label"":[""another""]}}", GetSettings())!;

        // assert
        result.Data.Is(5);
        result.IsSuccess.IsTrue();
        result.IsFailure.IsFalse();
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is("plain");
        result.LabeledErrors.Has(1);
        result.LabeledErrors.At("label").At(0).Is("another");
    }

    private JsonSerializerOptions GetSettings() => new JsonSerializerOptions().ConfigureForOperations();
}