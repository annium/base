using Annium.Architecture.Base;
using Annium.Testing;
using Xunit;

namespace Annium.Architecture.Base.Tests;

/// <summary>
/// Smoke tests for <see cref="OperationStatus"/>. Guards the set of members from accidental deletion
/// — the Http-status translation (Architecture.Http) and the response-mapping pipeline assume these
/// members exist by name, so removing one would break downstream packages silently at runtime.
/// </summary>
public class OperationStatusTests
{
    /// <summary>Every documented status member is present on the enum.</summary>
    [Fact]
    public void OperationStatus_ContainsExpectedMembers()
    {
        System.Enum.IsDefined(typeof(OperationStatus), nameof(OperationStatus.Ok)).IsTrue();
        System.Enum.IsDefined(typeof(OperationStatus), nameof(OperationStatus.BadRequest)).IsTrue();
        System.Enum.IsDefined(typeof(OperationStatus), nameof(OperationStatus.Unauthorized)).IsTrue();
        System.Enum.IsDefined(typeof(OperationStatus), nameof(OperationStatus.Forbidden)).IsTrue();
        System.Enum.IsDefined(typeof(OperationStatus), nameof(OperationStatus.NotFound)).IsTrue();
        System.Enum.IsDefined(typeof(OperationStatus), nameof(OperationStatus.Conflict)).IsTrue();
        System.Enum.IsDefined(typeof(OperationStatus), nameof(OperationStatus.NetworkError)).IsTrue();
        System.Enum.IsDefined(typeof(OperationStatus), nameof(OperationStatus.Aborted)).IsTrue();
        System.Enum.IsDefined(typeof(OperationStatus), nameof(OperationStatus.Timeout)).IsTrue();
        System.Enum.IsDefined(typeof(OperationStatus), nameof(OperationStatus.UncaughtError)).IsTrue();
    }
}
