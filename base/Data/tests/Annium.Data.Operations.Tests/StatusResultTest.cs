using Annium.Testing;
using Annium.Testing.Collection;
using Xunit;

namespace Annium.Data.Operations.Tests;

/// <summary>
/// Tests for StatusResult functionality and behavior.
/// </summary>
public class StatusResultTest
{
    /// <summary>
    /// Tests that a StatusResult without data correctly stores the status.
    /// </summary>
    [Fact]
    public void StatusResult_WithoutData_WorksCorrectly()
    {
        // arrange
        var result = Result.Status(Access.Allowed);

        // assert
        result.Status.Is(Access.Allowed);
    }

    /// <summary>
    /// Tests that a StatusResult with data correctly stores both status and data.
    /// </summary>
    [Fact]
    public void StatusResult_WithData_WorksCorrectly()
    {
        // arrange
        var result = Result.Status(Access.Allowed, 5);
        var (status, data) = result;

        // assert
        result.Status.Is(Access.Allowed);
        result.Data.Is(5);
        status.Is(Access.Allowed);
        data.Is(5);
    }

    /// <summary>
    /// Tests that cloning a StatusResult produces a valid copy with all properties preserved.
    /// </summary>
    [Fact]
    public void StatusResult_Clone_ReturnsValidClone()
    {
        // arrange
        var succeed = Result.Status(Access.Allowed);
        var failed = Result.Status(Access.Denied).Error("plain").Error("label", "value");

        // act
        var succeedClone = succeed.Copy();
        var failedClone = failed.Copy();

        // assert
        succeedClone.Status.Is(Access.Allowed);
        succeedClone.IsOk.IsTrue();
        failedClone.Status.Is(Access.Denied);
        failedClone.HasErrors.IsTrue();
        failedClone.PlainErrors.Has(1);
        failedClone.PlainErrors.At(0).Is("plain");
        failedClone.LabeledErrors.Has(1);
        failedClone.LabeledErrors.At("label").Has(1);
        failedClone.LabeledErrors.At("label").At(0).Is("value");
    }

    /// <summary>
    /// Tests that cloning a StatusResult with data produces a valid copy with all properties and data preserved.
    /// </summary>
    [Fact]
    public void StatusResult_CloneWithData_ReturnsValidClone()
    {
        // arrange
        var succeed = Result.Status(Access.Allowed, "welcome");
        var failed = Result.Status(Access.Denied, "goodbye").Error("plain").Error("label", "value");

        // act
        var succeedClone = succeed.Copy();
        var failedClone = failed.Copy();

        // assert
        succeedClone.Status.Is(Access.Allowed);
        succeedClone.IsOk.IsTrue();
        succeedClone.Data.Is("welcome");
        failedClone.Status.Is(Access.Denied);
        failedClone.HasErrors.IsTrue();
        failedClone.PlainErrors.Has(1);
        failedClone.PlainErrors.At(0).Is("plain");
        failedClone.LabeledErrors.Has(1);
        failedClone.LabeledErrors.At("label").Has(1);
        failedClone.LabeledErrors.At("label").At(0).Is("value");
        failedClone.Data.Is("goodbye");
    }

    private enum Access
    {
        Allowed,
        Denied,
        Error,
    }
}
