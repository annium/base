using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Tests;

public class BooleanResultTest
{
    [Fact]
    public void BooleanResult_Success_IsCorrect()
    {
        // arrange
        var result = Result.Success();

        // assert
        result.IsSuccess.IsTrue();
        result.IsFailure.IsFalse();
    }

    [Fact]
    public void BooleanResult_SuccessWithError_IsSuccess()
    {
        // arrange
        var result = Result.Success().Error("plain");

        // assert
        result.IsSuccess.IsTrue();
        result.IsFailure.IsFalse();
    }

    [Fact]
    public void BooleanResult_Failure_IsCorrect()
    {
        // arrange
        var result = Result.Failure();

        // assert
        result.IsSuccess.IsFalse();
        result.IsFailure.IsTrue();
    }

    [Fact]
    public void BooleanResult_SuccessWithData_IsCorrect()
    {
        // arrange
        var result = Result.Success(5);
        var (succeed, data) = result;

        // assert
        result.Data.Is(data);
        result.IsSuccess.IsTrue();
        result.IsFailure.IsFalse();
        data.Is(5);
        succeed.IsTrue();
    }

    [Fact]
    public void BooleanResult_SuccessWithDataWithError_IsSuccess()
    {
        // arrange
        var result = Result.Success(5).Error("plain");
        var (succeed, data) = result;

        // assert
        result.Data.Is(data);
        result.IsSuccess.IsTrue();
        result.IsFailure.IsFalse();
        data.Is(5);
        succeed.IsTrue();
    }

    [Fact]
    public void BooleanResult_FailureWithData_IsCorrect()
    {
        // arrange
        var result = Result.Failure(5);
        var (succeed, data) = result;

        // assert
        result.Data.Is(data);
        result.IsSuccess.IsFalse();
        result.IsFailure.IsTrue();
        data.Is(5);
        succeed.IsFalse();
    }

    [Fact]
    public void BooleanResult_Clone_ReturnsValidClone()
    {
        // arrange
        var succeed = Result.Success();
        var failed = Result.Failure().Error("plain").Error("label", "value");

        // act
        var succeedClone = succeed.Copy();
        var failedClone = failed.Copy();

        // assert
        succeedClone.IsSuccess.IsTrue();
        succeedClone.IsOk.IsTrue();
        failedClone.IsFailure.IsTrue();
        failedClone.HasErrors.IsTrue();
        failedClone.PlainErrors.Has(1);
        failedClone.PlainErrors.At(0).Is("plain");
        failedClone.LabeledErrors.Has(1);
        failedClone.LabeledErrors.At("label").Has(1);
        failedClone.LabeledErrors.At("label").At(0).Is("value");
    }

    [Fact]
    public void BooleanResult_CloneWithData_ReturnsValidClone()
    {
        // arrange
        var succeed = Result.Success("x");
        var failed = Result.Failure(10).Error("plain").Error("label", "value");

        // act
        var succeedClone = succeed.Copy();
        var failedClone = failed.Copy();

        // assert
        succeedClone.IsSuccess.IsTrue();
        succeedClone.IsOk.IsTrue();
        succeedClone.Data.Is("x");
        failedClone.IsFailure.IsTrue();
        failedClone.HasErrors.IsTrue();
        failedClone.PlainErrors.Has(1);
        failedClone.PlainErrors.At(0).Is("plain");
        failedClone.LabeledErrors.Has(1);
        failedClone.LabeledErrors.At("label").Has(1);
        failedClone.LabeledErrors.At("label").At(0).Is("value");
        failedClone.Data.Is(10);
    }
}