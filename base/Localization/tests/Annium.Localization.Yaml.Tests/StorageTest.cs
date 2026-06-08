using System.Globalization;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Localization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Localization.Yaml.Tests;

/// <summary>
/// Tests for YAML-based localization storage functionality.
/// Validates locale loading and translation retrieval from YAML files.
/// </summary>
public class StorageTest : TestBase
{
    /// <summary>
    /// Captures the ambient culture so each test can restore it, preventing
    /// CultureInfo.CurrentCulture mutations from leaking across tests.
    /// </summary>
    private readonly CultureInfo _savedCulture = CultureInfo.CurrentCulture;

    /// <summary>
    /// Initializes a new instance of the StorageTest class, registering YAML localization.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public StorageTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddLocalization(opts => opts.UseYamlStorage()));
    }

    /// <summary>
    /// Tests basic localization functionality with YAML storage.
    /// Verifies that localizer correctly retrieves translations from YAML-based locale files.
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
    /// Verifies that looking up any key when no locale file exists for the current culture returns
    /// the raw key rather than throwing.
    /// </summary>
    [Fact]
    public void LoadLocale_MissingFile_ReturnsKey()
    {
        // arrange
        var localizer = Get<ILocalizer<StorageTest>>();

        // fr has no fr.yml fixture → every lookup is a miss
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr");

        // act
        var result = localizer["test"];

        // assert
        result.Is("test");
    }

    /// <summary>
    /// Verifies that a malformed YAML file (unterminated quoted scalar) degrades gracefully:
    /// the storage swallows the <c>YamlException</c> and returns the raw key.
    /// </summary>
    [Fact]
    public void LoadLocale_MalformedYaml_ReturnsKey()
    {
        // arrange
        var localizer = Get<ILocalizer<StorageTest>>();

        // de.yml contains an unterminated quoted scalar → YamlException on parse
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de");

        // act
        var result = localizer["test"];

        // assert
        result.Is("test");
    }

    /// <summary>
    /// Verifies that a YAML key with an explicit null value returns the raw key while a sibling
    /// key with a valid value returns the translated string.
    /// </summary>
    [Fact]
    public void LoadLocale_NullValueKey_ReturnsKey()
    {
        // arrange
        var localizer = Get<ILocalizer<StorageTest>>();

        // it.yml: "test: demo" (valid) and "empty:" (null value → miss)
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("it");

        // act
        var emptyResult = localizer["empty"];
        var testResult = localizer["test"];

        // assert
        emptyResult.Is("empty");
        testResult.Is("demo");
    }

    /// <summary>
    /// Verifies that an empty YAML file (deserializes to null) degrades gracefully: the null is
    /// coalesced to an empty locale and a key lookup returns the raw key rather than throwing.
    /// </summary>
    [Fact]
    public void LoadLocale_EmptyFile_ReturnsKey()
    {
        // arrange
        var localizer = Get<ILocalizer<StorageTest>>();

        // es.yml is an empty fixture → Deserialize returns null → coalesced to empty
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es");

        // act
        var result = localizer["test"];

        // assert
        result.Is("test");
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
