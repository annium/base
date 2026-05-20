using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for nullable extension methods.
/// </summary>
public class NullableExtensionsTests
{
    /// <summary>
    /// Verifies that NotNull throws for null class references and returns the value otherwise.
    /// </summary>
    [Fact]
    public void EnsureNotNull_Class()
    {
        // arrange
        string? nullValue = null;
        string? validValue = null;
        validValue.IsDefault();
        validValue = "data";

        // assert
        Wrap.It(() =>
            {
                var failedValue = nullValue.NotNull();
                var s = new string(failedValue);
            })
            .Throws<NullReferenceException>()
            .Reports($"{nameof(nullValue)} is null");

        var verifiedValue = validValue.NotNull();
        verifiedValue.Is(validValue);
    }

    /// <summary>
    /// Verifies that NotNull throws for null struct values and returns the value otherwise.
    /// </summary>
    [Fact]
    public void EnsureNotNull_Struct()
    {
        // arrange
        bool? nullValue = null;
        bool? validValue = true;

        // assert
        Wrap.It(() =>
            {
                var failedValue = nullValue.NotNull();
            })
            .Throws<NullReferenceException>()
            .Reports($"{nameof(nullValue)} is null");

        var verifiedValue = validValue.NotNull();
        verifiedValue.Is(validValue.Value);
    }

    /// <summary>
    /// Verifies that NotNullAsync throws for null class Task results and returns the value otherwise.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EnsureNotNull_ClassTask()
    {
        // arrange
        var nullValue = Task.FromResult<string?>(null);
        var validValue = Task.FromResult<string?>("data");

        // assert
        // VSTHRD003: awaiting caller-provided Task<T?> to exercise NotNullAsync — analyzer false positive in test context.
#pragma warning disable VSTHRD003
        await Wrap.It(async () => await nullValue.NotNullAsync())
#pragma warning restore VSTHRD003
            .ThrowsAsync<NullReferenceException>()
            .ReportsAsync($"{nameof(nullValue)} is null");

        var verifiedValue = await validValue.NotNullAsync();
        verifiedValue.Is("data");
    }

    /// <summary>
    /// Verifies that NotNullAsync throws for null struct Task results and returns the value otherwise.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EnsureNotNull_StructTask()
    {
        // arrange
        var nullValue = Task.FromResult<bool?>(null);
        var validValue = Task.FromResult<bool?>(true);

        // assert
        await Wrap.It(async () =>
            {
                try
                {
                    // VSTHRD003: awaiting caller-provided Task<T?> to exercise NotNullAsync — analyzer false positive in test context.
#pragma warning disable VSTHRD003
                    var failedValue = await nullValue.NotNullAsync();
#pragma warning restore VSTHRD003
                }
                catch (AggregateException ex)
                {
                    throw ex.InnerExceptions.Single();
                }
            })
            .ThrowsAsync<NullReferenceException>()
            .ReportsAsync($"{nameof(nullValue)} is null");

        var verifiedValue = await validValue.NotNullAsync();
        verifiedValue.Is(true);
    }

    /// <summary>
    /// Verifies that NotNullAsync on a ValueTask&lt;T?&gt; struct overload throws for a null result and
    /// returns the value otherwise. Closes the TG6 ValueTask-overload gap from review-2026.05.15.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EnsureNotNullAsync_ValueTask_Struct()
    {
        var nullValue = new ValueTask<bool?>((bool?)null);
        var validValue = new ValueTask<bool?>((bool?)true);

        await Wrap.It(async () => _ = await nullValue.NotNullAsync()).ThrowsAsync<NullReferenceException>();

        var verified = await validValue.NotNullAsync();
        verified.Is(true);
    }

    /// <summary>
    /// Verifies that NotNullAsync on a ValueTask&lt;T?&gt; reference (class) overload throws for a null
    /// result and returns the value otherwise. Closes the TG6 ValueTask reference-overload gap.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task EnsureNotNullAsync_ValueTask_Class()
    {
        var nullValue = new ValueTask<string?>((string?)null);
        var validValue = new ValueTask<string?>("data");

        await Wrap.It(async () => _ = await nullValue.NotNullAsync()).ThrowsAsync<NullReferenceException>();

        var verified = await validValue.NotNullAsync();
        verified.Is("data");
    }
}
