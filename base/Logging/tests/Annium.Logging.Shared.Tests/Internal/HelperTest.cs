using System;
using Annium.Logging.Shared.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Logging.Shared.Tests.Internal;

/// <summary>
/// Tests for the Helper class message template processing
/// </summary>
public class HelperTest
{
    /// <summary>
    /// Tests that normal message template processing works correctly
    /// </summary>
    [Fact]
    public void Normal_Works()
    {
        // arrange
        var user = "alex";
        var time = "01.01.2021 08:00";

        // act
        var (message, data) = Helper.Process("Log-in by {user} at {time}", new object[] { user, time });

        // assert
        message.Is("Log-in by alex at 01.01.2021 08:00");
        data.Has(2);
        data.At("user").As<string>().Is(user);
        data.At("time").As<string>().Is(time);
    }

    /// <summary>
    /// Tests that corner case message template processing works correctly
    /// </summary>
    [Fact]
    public void Corner_Works()
    {
        // arrange
        var user = "alex";
        var time = "01.01.2021 08:00";

        // act
        var (message, data) = Helper.Process("{user}{time}", new object[] { user, time });

        // assert
        message.Is("alex01.01.2021 08:00");
        data.Has(2);
        data.At("user").As<string>().Is(user);
        data.At("time").As<string>().Is(time);
    }

    /// <summary>
    /// Tests that nested placeholders without data are ignored correctly
    /// </summary>
    [Fact]
    public void Nested_NoData_IgnoredCorrectly()
    {
        // act
        var (message, data) = Helper.Process("some {{nested} data} here", Array.Empty<object>());

        // assert
        message.Is("some {{nested} data} here");
        data.IsEmpty();
    }

    /// <summary>
    /// Tests that nested placeholders with data work correctly
    /// </summary>
    [Fact]
    public void Nested_WithData_Works()
    {
        // act
        var (message, data) = Helper.Process("some {{nested} data} here", new object[] { "demo" });

        // assert
        message.Is("some demo here");
        data.Has(1);
        data.At("{nested} data").As<string>().Is("demo");
    }

    /// <summary>
    /// When more data items are supplied than the template has placeholders,
    /// <see cref="Helper.Process"/> must throw <see cref="InvalidOperationException"/>.
    /// The guard is on line ~67-69 of Helper.cs: <c>if (extra > 0) throw ...</c>.
    /// This test fails if that guard is removed or weakened.
    /// </summary>
    [Fact]
    public void ExtraDataItems_ThrowsInvalidOperationException()
    {
        Wrap.It(() => Helper.Process("hello {name}", new object?[] { "alex", "extra" }))
            .Throws<InvalidOperationException>();
    }

    /// <summary>
    /// When data items exactly match the template placeholders,
    /// <see cref="Helper.Process"/> must succeed and substitute the value.
    /// Verifies the exact-match path does not throw and produces the expected message and data.
    /// </summary>
    [Fact]
    public void ExactDataItems_DoesNotThrow_ProducesExpected()
    {
        var (message, data) = Helper.Process("hello {name}", new object?[] { "alex" });

        message.Is("hello alex");
        data.Has(1);
        data.At("name").As<string>().Is("alex");
    }
}
