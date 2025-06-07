using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Tests;

/// <summary>
/// Tests for BooleanResult functionality and behavior.
/// </summary>
public class BooleanResultTest
{
    /// <summary>
    /// Tests that a success BooleanResult has correct state properties.
    /// </summary>
    [Fact]
    public void BooleanResult_Success_IsCorrect()
    {
        // arrange
        var result = Result.Success();

        // assert
        result.IsSuccess.IsTrue();
        result.IsFailure.IsFalse();
    }

    /// <summary>
    /// Tests that a success BooleanResult remains successful even when errors are added.
    /// </summary>
    [Fact]
    public void BooleanResult_SuccessWithError_IsSuccess()
    {
        // arrange
        var result = Result.Success().Error("plain");

        // assert
        result.IsSuccess.IsTrue();
        result.IsFailure.IsFalse();
    }

    /// <summary>
    /// Tests that a failure BooleanResult has correct state properties.
    /// </summary>
    [Fact]
    public void BooleanResult_Failure_IsCorrect()
    {
        // arrange
        var result = Result.Failure();

        // assert
        result.IsSuccess.IsFalse();
        result.IsFailure.IsTrue();
    }

    /// <summary>
    /// Tests that a success BooleanResult with data has correct state and data properties.
    /// </summary>
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

    /// <summary>
    /// Tests that a success BooleanResult with data remains successful even when errors are added.
    /// </summary>
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

    /// <summary>
    /// Tests that a failure BooleanResult with data has correct state and data properties.
    /// </summary>
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

    /// <summary>
    /// Tests that cloning a BooleanResult produces a valid copy with all properties preserved.
    /// </summary>
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

    /// <summary>
    /// Tests that cloning a BooleanResult with data produces a valid copy with all properties and data preserved.
    /// </summary>
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
