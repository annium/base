using System.Linq;
using System.Text;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Resources;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Runtime.Tests.Resources;

/// <summary>
/// Tests for <see cref="IResourceLoader"/>, which loads embedded resources from assemblies.
/// </summary>
/// <remarks>
/// Fixtures are embedded in this assembly under the manifest prefix
/// <c>Annium.Core.Runtime.Tests.Resources.Fixtures.</c>. The loader is
/// given prefix <c>Resources.Fixtures.</c> (the assembly short-name segment is
/// prepended internally), so the returned <see cref="IResource.Name"/> values
/// have that full prefix stripped.
/// </remarks>
public class ResourceLoaderTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="ResourceLoaderTests"/> and registers
    /// the resource loader with the DI container.
    /// </summary>
    /// <param name="outputHelper">The xunit output helper.</param>
    public ResourceLoaderTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddResourceLoader());
    }

    /// <summary>
    /// When a prefix matches an embedded fixture, Load returns exactly one resource
    /// whose name is the filename with the prefix stripped and whose content matches
    /// the known fixture bytes.
    /// </summary>
    [Fact]
    public void Load_WithMatchingPrefix_ReturnsResourceWithStrippedNameAndContent()
    {
        // arrange
        var loader = Get<IResourceLoader>();
        var assembly = typeof(ResourceLoaderTests).Assembly;
        const string expectedName = "sample.txt";
        const string expectedText = "hello embedded resource";
        var expectedBytes = Encoding.UTF8.GetBytes(expectedText);

        // act
        var resources = loader.Load("Resources.Fixtures.", assembly);

        // assert — exactly one resource returned
        resources.Has(1);
        var resource = resources.At(0);

        // name has the prefix stripped: only the bare filename remains
        resource.Name.Is(expectedName);

        // content matches the known fixture text bytes (SequenceEqual for value comparison of arrays)
        resource.Content.ToArray().SequenceEqual(expectedBytes).IsTrue();
    }

    /// <summary>
    /// When the prefix does not match any embedded resource in the assembly,
    /// Load returns an empty collection.
    /// </summary>
    [Fact]
    public void Load_WithNonMatchingPrefix_ReturnsEmpty()
    {
        // arrange
        var loader = Get<IResourceLoader>();
        var assembly = typeof(ResourceLoaderTests).Assembly;

        // act
        var resources = loader.Load("NonExistent.Prefix.", assembly);

        // assert
        resources.IsEmpty();
    }
}
