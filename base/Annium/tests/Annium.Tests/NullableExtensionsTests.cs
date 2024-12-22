using System;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class NullableExtensionsTests
{
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
