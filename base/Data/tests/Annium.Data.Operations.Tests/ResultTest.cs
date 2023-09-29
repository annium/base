using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Tests;

public class ResultTest
{
    [Fact]
    public void Blank_HasNoErrors()
    {
        // arrange
        var result = Result.New();

        // assert
        result.IsOk.IsTrue();
    }

    [Fact]
    public void Blank_WithData_IsCorrect()
    {
        // arrange
        var result = Result.New(5);

        // assert
        result.IsOk.IsTrue();
        result.Data.Is(5);
    }

    [Fact]
    public void Clear_RemovesErrors()
    {
        // arrange
        var result = Result.New().Error("plain").Error("label", "value");

        // act
        result.Clear();

        // assert
        result.IsOk.IsTrue();
    }

    [Fact]
    public void PlainError_IsAddedToPlainErrors()
    {
        // arrange
        var result = Result.New();

        // act
        result.Error("plain");

        // assert
        result.HasErrors.IsTrue();
        result.PlainErrors.Has(1);
        result.PlainErrors.At(0).Is("plain");
    }

    [Fact]
    public void LabeledError_IsAddedToLabeledErrors()
    {
        // arrange
        var result = Result.New();

        // act
        result.Error("label", "plain");

        // assert
        result.HasErrors.IsTrue();
        result.LabeledErrors.Count.Is(1);
        result.LabeledErrors.At("label").At(0).Is("plain");
    }

    [Fact]
    public void PlainErrors_Params_IsAddedCorrectly()
    {
        // arrange
        var result = Result.New();

        // act
        result.Errors("plain", "another", "another");

        // assert
        result.PlainErrors.Has(2);
        result.PlainErrors.At(0).Is("plain");
        result.PlainErrors.At(1).Is("another");
    }

    [Fact]
    public void PlainErrors_Collection_IsAddedCorrectly()
    {
        // arrange
        var result = Result.New();

        // act
        result.Errors(new List<string> { "plain", "another", "another" });

        // assert
        result.PlainErrors.Has(2);
        result.PlainErrors.At(0).Is("plain");
        result.PlainErrors.At(1).Is("another");
    }

    [Fact]
    public void LabeledErrors_Params_IsAddedCorrectly()
    {
        // arrange
        var result = Result.New();

        // act
        result.Errors(("label", new[] { "plain" }), ("other", new[] { "prev" }), ("other", new[] { "another" }));

        // assert
        result.LabeledErrors.Count.Is(2);
        result.LabeledErrors.At("label").At(0).Is("plain");
        result.LabeledErrors.At("other").At(0).Is("prev");
        result.LabeledErrors.At("other").At(1).Is("another");
    }

    [Fact]
    public void LabeledErrors_Collection_IsAddedCorrectly()
    {
        // arrange
        var result = Result.New();

        // act
        result.Errors(new Dictionary<string, IReadOnlyCollection<string>> { { "label", new[] { "plain" } }, { "other", new[] { "another" } } });

        // assert
        result.LabeledErrors.Count.Is(2);
        result.LabeledErrors.At("label").At(0).Is("plain");
        result.LabeledErrors.At("other").At(0).Is("another");
    }

    [Fact]
    public void Join_Params_IsDoneCorrectly()
    {
        // arrange
        var result = Result.New().Error("own").Error("label", "mine");
        var plain = Result.New().Errors("plain", "another");
        var labeled = Result.New().Errors(("a", new[] { "va" }), ("b", new[] { "vb" }));

        // act
        result.Join(plain, labeled);

        // assert
        result.HasErrors.IsTrue();
        result.PlainErrors.Has(3);
        result.PlainErrors.At(0).Is("own");
        result.PlainErrors.At(1).Is("plain");
        result.PlainErrors.At(2).Is("another");
        result.LabeledErrors.Count.Is(3);
        result.LabeledErrors.At("label").At(0).Is("mine");
        result.LabeledErrors.At("a").At(0).Is("va");
        result.LabeledErrors.At("b").At(0).Is("vb");
    }

    [Fact]
    public void Join_Collection_IsDoneCorrectly()
    {
        // arrange
        var result = Result.New().Error("own").Error("label", "mine");
        var plain = Result.New().Errors("plain", "another");
        var labeled = Result.New().Errors(("a", new[] { "va" }), ("b", new[] { "vb" }));

        // act
        result.Join(new List<IResult> { plain, labeled });

        // assert
        result.HasErrors.IsTrue();
        result.PlainErrors.Has(3);
        result.PlainErrors.At(0).Is("own");
        result.PlainErrors.At(1).Is("plain");
        result.PlainErrors.At(2).Is("another");
        result.LabeledErrors.Count.Is(3);
        result.LabeledErrors.At("label").At(0).Is("mine");
        result.LabeledErrors.At("a").At(0).Is("va");
        result.LabeledErrors.At("b").At(0).Is("vb");
    }

    [Fact]
    public void Result_Clone_ReturnsValidClone()
    {
        // arrange
        var result = Result.New().Error("plain").Error("label", "value");

        // act
        var clone = result.Copy();

        // assert
        clone.HasErrors.IsTrue();
        clone.HasErrors.IsTrue();
        clone.PlainErrors.Has(1);
        clone.PlainErrors.At(0).Is("plain");
        clone.LabeledErrors.Has(1);
        clone.LabeledErrors.At("label").Has(1);
        clone.LabeledErrors.At("label").At(0).Is("value");
    }

    [Fact]
    public void Result_CloneWithData_ReturnsValidClone()
    {
        // arrange
        var result = Result.New(10).Error("plain").Error("label", "value");

        // act
        var clone = result.Copy();

        // assert
        clone.Data.Is(10);
        clone.HasErrors.IsTrue();
        clone.PlainErrors.Has(1);
        clone.PlainErrors.At(0).Is("plain");
        clone.LabeledErrors.Has(1);
        clone.LabeledErrors.At("label").Has(1);
        clone.LabeledErrors.At("label").At(0).Is("value");
        clone.Data.Is(10);
    }

    [Fact]
    public void JoinStatic_Params_IsDoneCorrectly()
    {
        // arrange
        var result = Result.New().Error("own").Error("label", "mine");
        var plain = Result.New().Errors("plain", "another");
        var labeled = Result.New().Errors(("a", new[] { "va" }), ("b", new[] { "vb" }));

        // act
        var output = Result.Join(result, plain, labeled);

        // assert
        output.HasErrors.IsTrue();
        output.PlainErrors.Has(3);
        output.PlainErrors.At(0).Is("own");
        output.PlainErrors.At(1).Is("plain");
        output.PlainErrors.At(2).Is("another");
        output.LabeledErrors.Count.Is(3);
        output.LabeledErrors.At("label").At(0).Is("mine");
        output.LabeledErrors.At("a").At(0).Is("va");
        output.LabeledErrors.At("b").At(0).Is("vb");
    }

    [Fact]
    public void JoinStatic_Collection_IsDoneCorrectly()
    {
        // arrange
        var result = Result.New().Error("own").Error("label", "mine");
        var plain = Result.New().Errors("plain", "another");
        var labeled = Result.New().Errors(("a", new[] { "va" }), ("b", new[] { "vb" }));

        // act
        var output = Result.Join(new List<IResult> { result, plain, labeled });

        // assert
        output.HasErrors.IsTrue();
        output.PlainErrors.Has(3);
        output.PlainErrors.At(0).Is("own");
        output.PlainErrors.At(1).Is("plain");
        output.PlainErrors.At(2).Is("another");
        output.LabeledErrors.Count.Is(3);
        output.LabeledErrors.At("label").At(0).Is("mine");
        output.LabeledErrors.At("a").At(0).Is("va");
        output.LabeledErrors.At("b").At(0).Is("vb");
    }
}