using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Localization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Localization.InMemory.Tests;

/// <summary>
/// Tests for in-memory localization storage functionality.
/// Validates locale loading and translation retrieval from memory-based storage.
/// </summary>
public class StorageTest : TestBase
{
    /// <summary>
    /// Captures the ambient culture so each test can restore it, preventing
    /// CultureInfo.CurrentCulture mutations from leaking across tests.
    /// </summary>
    private readonly CultureInfo _savedCulture = CultureInfo.CurrentCulture;

    /// <summary>
    /// Initializes a new instance of the StorageTest class, registering in-memory localization.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public StorageTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        var locales = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>();
        locales[CultureInfo.GetCultureInfo("en")] = new Dictionary<string, string> { { "test", "demo" } };
        locales[CultureInfo.GetCultureInfo("ru")] = new Dictionary<string, string> { { "test", "демо" } };

        Register(container => container.AddLocalization(opts => opts.UseInMemoryStorage(locales)));
    }

    /// <summary>
    /// Tests basic localization functionality with in-memory storage.
    /// Verifies that localizer correctly retrieves translations from memory-based locale storage.
    /// </summary>
    [Fact]
    public void Localization_Works()
    {
        // arrange
        var localizer = Get<ILocalizer<StorageTest>>();

        // act
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
        var en = localizer["test"];
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
        var ru = localizer["test"];

        // assert
        en.Is("demo");
        ru.Is("демо");
    }

    /// <summary>
    /// Restores the ambient culture mutated during the test.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that completes when base disposal finishes.</returns>
    public override ValueTask DisposeAsync()
    {
        CultureInfo.CurrentCulture = _savedCulture;
        return base.DisposeAsync();
    }
}
