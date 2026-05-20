using System;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for <see cref="ExceptionExtensions"/> to verify Rethrow behavior.
/// </summary>
public class ExceptionExtensionsTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionExtensionsTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ExceptionExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that Rethrow preserves the original stack trace of the captured exception.
    /// The helper method name must appear in the stack trace even after rethrowing from a different frame.
    /// </summary>
    [Fact]
    public void Rethrow_PreservesOriginalStackTrace()
    {
        // arrange — capture an exception thrown from a named helper so we can verify its frame survives
        Exception captured;
        try
        {
            ThrowFromNamedHelper();
            captured = null!; // unreachable
        }
        catch (InvalidOperationException ex)
        {
            captured = ex;
        }

        // act — rethrow from this frame (a different frame)
        Exception? rethrown = null;
        try
        {
            captured.Rethrow();
        }
        catch (InvalidOperationException ex)
        {
            rethrown = ex;
        }

        // assert — rethrown is not null and the original helper frame is still in the stack trace
        (rethrown is not null).IsTrue();
        (rethrown!.StackTrace is not null).IsTrue();
        rethrown.StackTrace!.Contains(nameof(ThrowFromNamedHelper)).IsTrue();
    }

    /// <summary>
    /// Verifies that the exception thrown by Rethrow is the same object instance as the original.
    /// </summary>
    [Fact]
    public void Rethrow_ReturnsSameException()
    {
        // arrange
        var original = new InvalidOperationException("identity-check");

        // act
        Exception? caught = null;
        try
        {
            original.Rethrow();
        }
        catch (InvalidOperationException ex)
        {
            caught = ex;
        }

        // assert — reference equality: same object, not a copy
        (caught is not null).IsTrue();
        ReferenceEquals(original, caught).IsTrue();
    }

    /// <summary>Helper that throws so <see cref="Rethrow_PreservesOriginalStackTrace"/> can capture the frame.</summary>
    private static void ThrowFromNamedHelper() => throw new InvalidOperationException("original");
}
