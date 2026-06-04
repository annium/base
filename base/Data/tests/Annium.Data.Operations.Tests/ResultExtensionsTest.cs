using System;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Operations.Tests;

/// <summary>
/// Tests for extension methods on result types: ThrowIfHasErrors, Unwrap, EnsureSuccess, EnsureFailure, EnsureHasStatus.
/// Also covers PlainError concatenation and Copy isolation.
/// </summary>
public class ResultExtensionsTest
{
    // -------------------------------------------------------------------------
    // ThrowIfHasErrors
    // -------------------------------------------------------------------------

    /// <summary>
    /// ThrowIfHasErrors throws InvalidOperationException when the result has errors.
    /// </summary>
    [Fact]
    public void ThrowIfHasErrors_WithErrors_Throws()
    {
        // arrange
        var result = Result.Create().Error("something went wrong");

        // act / assert
        Wrap.It(() => result.ThrowIfHasErrors()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// ThrowIfHasErrors does not throw when the result has no errors.
    /// </summary>
    [Fact]
    public void ThrowIfHasErrors_NoErrors_DoesNotThrow()
    {
        // arrange
        var result = Result.Create();

        // act / assert — must not throw
        result.ThrowIfHasErrors();
        result.IsOk.IsTrue();
    }

    // -------------------------------------------------------------------------
    // Unwrap<T>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Unwrap returns the data when the result has no errors.
    /// </summary>
    [Fact]
    public void Unwrap_OkResult_ReturnsData()
    {
        // arrange
        var result = Result.Create(42);

        // act
        var data = result.Unwrap();

        // assert
        data.Is(42);
    }

    /// <summary>
    /// Unwrap throws InvalidOperationException when the result has errors.
    /// </summary>
    [Fact]
    public void Unwrap_ErrorResult_Throws()
    {
        // arrange
        var result = Result.Create(42).Error("bad input");

        // act / assert
        Wrap.It(() => result.Unwrap()).Throws<InvalidOperationException>();
    }

    // -------------------------------------------------------------------------
    // EnsureSuccess (non-generic IBooleanResult)
    // -------------------------------------------------------------------------

    /// <summary>
    /// EnsureSuccess returns the same result instance when it represents success.
    /// </summary>
    [Fact]
    public void EnsureSuccess_OnSuccess_ReturnsResult()
    {
        // arrange
        var result = Result.Success();

        // act
        var returned = result.EnsureSuccess();

        // assert
        returned.IsSuccess.IsTrue();
        ReferenceEquals(result, returned).IsTrue();
    }

    /// <summary>
    /// EnsureSuccess throws InvalidOperationException when the result represents failure.
    /// </summary>
    [Fact]
    public void EnsureSuccess_OnFailure_Throws()
    {
        // arrange
        var result = Result.Failure();

        // act / assert
        Wrap.It(() => result.EnsureSuccess()).Throws<InvalidOperationException>();
    }

    // -------------------------------------------------------------------------
    // EnsureSuccess<T> (generic IBooleanResult<T>)
    // -------------------------------------------------------------------------

    /// <summary>
    /// EnsureSuccess&lt;T&gt; returns the same result instance when it represents success.
    /// </summary>
    [Fact]
    public void EnsureSuccessGeneric_OnSuccess_ReturnsResult()
    {
        // arrange
        var result = Result.Success("hello");

        // act
        var returned = result.EnsureSuccess();

        // assert
        returned.IsSuccess.IsTrue();
        returned.Data.Is("hello");
        ReferenceEquals(result, returned).IsTrue();
    }

    /// <summary>
    /// EnsureSuccess&lt;T&gt; throws InvalidOperationException when the result represents failure.
    /// </summary>
    [Fact]
    public void EnsureSuccessGeneric_OnFailure_Throws()
    {
        // arrange
        var result = Result.Failure("hello");

        // act / assert
        Wrap.It(() => result.EnsureSuccess()).Throws<InvalidOperationException>();
    }

    // -------------------------------------------------------------------------
    // EnsureFailure (non-generic IBooleanResult)
    // -------------------------------------------------------------------------

    /// <summary>
    /// EnsureFailure returns the same result instance when it represents failure.
    /// </summary>
    [Fact]
    public void EnsureFailure_OnFailure_ReturnsResult()
    {
        // arrange
        var result = Result.Failure();

        // act
        var returned = result.EnsureFailure();

        // assert
        returned.IsFailure.IsTrue();
        ReferenceEquals(result, returned).IsTrue();
    }

    /// <summary>
    /// EnsureFailure throws InvalidOperationException when the result represents success.
    /// </summary>
    [Fact]
    public void EnsureFailure_OnSuccess_Throws()
    {
        // arrange
        var result = Result.Success();

        // act / assert
        Wrap.It(() => result.EnsureFailure()).Throws<InvalidOperationException>();
    }

    // -------------------------------------------------------------------------
    // EnsureFailure<T> (generic IBooleanResult<T>)
    // -------------------------------------------------------------------------

    /// <summary>
    /// EnsureFailure&lt;T&gt; returns the same result instance when it represents failure.
    /// </summary>
    [Fact]
    public void EnsureFailureGeneric_OnFailure_ReturnsResult()
    {
        // arrange
        var result = Result.Failure(99);

        // act
        var returned = result.EnsureFailure();

        // assert
        returned.IsFailure.IsTrue();
        returned.Data.Is(99);
        ReferenceEquals(result, returned).IsTrue();
    }

    /// <summary>
    /// EnsureFailure&lt;T&gt; throws InvalidOperationException when the result represents success.
    /// </summary>
    [Fact]
    public void EnsureFailureGeneric_OnSuccess_Throws()
    {
        // arrange
        var result = Result.Success(99);

        // act / assert
        Wrap.It(() => result.EnsureFailure()).Throws<InvalidOperationException>();
    }

    // -------------------------------------------------------------------------
    // EnsureHasStatus (IStatusResult<TS>)
    // -------------------------------------------------------------------------

    /// <summary>
    /// EnsureHasStatus returns the same result instance when the status matches.
    /// </summary>
    [Fact]
    public void EnsureHasStatus_MatchingStatus_ReturnsResult()
    {
        // arrange
        var result = Result.Status(HttpStatus.Ok);

        // act
        var returned = result.EnsureHasStatus(HttpStatus.Ok);

        // assert
        returned.Status.Is(HttpStatus.Ok);
        ReferenceEquals(result, returned).IsTrue();
    }

    /// <summary>
    /// EnsureHasStatus throws InvalidOperationException when the status does not match.
    /// </summary>
    [Fact]
    public void EnsureHasStatus_NonMatchingStatus_Throws()
    {
        // arrange
        var result = Result.Status(HttpStatus.NotFound);

        // act / assert
        Wrap.It(() => result.EnsureHasStatus(HttpStatus.Ok)).Throws<InvalidOperationException>();
    }

    // -------------------------------------------------------------------------
    // PlainError concatenation
    // -------------------------------------------------------------------------

    /// <summary>
    /// PlainError returns an empty string when there are no errors.
    /// </summary>
    [Fact]
    public void PlainError_NoErrors_ReturnsEmpty()
    {
        // arrange
        var result = Result.Create();

        // assert
        result.PlainError.Is(string.Empty);
    }

    /// <summary>
    /// PlainError joins multiple plain errors with "; " as separator.
    /// </summary>
    [Fact]
    public void PlainError_MultipleErrors_JoinsWithSeparator()
    {
        // arrange
        var result = Result.Create().Error("alpha").Error("beta");

        // act
        var plainError = result.PlainError;

        // assert — two errors joined by "; " (order not guaranteed for HashSet)
        result.PlainErrors.Has(2);
        plainError.Contains("alpha").IsTrue();
        plainError.Contains("beta").IsTrue();
        plainError.Contains("; ").IsTrue();
    }

    // -------------------------------------------------------------------------
    // Copy isolation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Copy produces an independent clone: adding errors to the original after copying does not affect the copy.
    /// </summary>
    [Fact]
    public void Copy_IsolatedFromOriginal()
    {
        // arrange
        var original = Result.Create().Error("original error");
        var copy = original.Copy();

        // act — mutate original after cloning
        original.Error("added after copy");

        // assert — copy is unaffected
        copy.PlainErrors.Has(1);
        copy.PlainErrors.At(0).Is("original error");
        original.PlainErrors.Has(2);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private enum HttpStatus
    {
        Ok,
        NotFound,
        ServerError,
    }
}
