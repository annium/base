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
#pragma warning disable VSTHRD003
        await Wrap.It(() => nullValue.NotNullAsync())
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
}
