using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests.Resolvers;

/// <summary>
/// Tests for resolution-based mapping in the mapper, including signature and ID-based resolution.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Resolve types based on their signatures
/// - Resolve types based on their IDs
/// - Handle polymorphic collections
/// - Map between different class hierarchies
/// - Preserve data during resolution-based mapping
/// </remarks>
public class ResolutionMapResolverTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResolutionMapResolverTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public ResolutionMapResolverTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Tests that signature-based resolution works for polymorphic collections.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Objects are correctly resolved based on their signatures
    /// - Polymorphic collections are properly mapped
    /// - Property values are preserved during mapping
    /// - The order of items in the collection is maintained
    /// </remarks>
    [Fact]
    public void SignatureResolution_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Payload[]
        {
            new ImagePayload("img"),
            new LinkPayload { Link = "lnk" },
        };

        // act
        var result = mapper.Map<List<Model>>(value);

        // assert
        result.Has(2);
        result.At(0).As<ImageModel>().Image.Is("img");
        result.At(1).As<LinkModel>().Link.Is("lnk");
    }

    /// <summary>
    /// Tests that ID-based resolution works for polymorphic collections.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Objects are correctly resolved based on their IDs
    /// - Polymorphic collections are properly mapped
    /// - Property values are preserved during mapping
    /// - The order of items in the collection is maintained
    /// </remarks>
    [Fact]
    public void IdResolution_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Req[]
        {
            new ImageReq("img"),
            new LinkReq { Data = "lnk" },
        };

        // act
        var result = mapper.Map<List<Mod>>(value);

        // assert
        result.Has(2);
        result.At(0).As<ImageMod>().Data.Is("img");
        result.At(1).As<LinkMod>().Data.Is("lnk");
    }

    /// <summary>
    /// Base class for payload types in the source hierarchy.
    /// </summary>
    private abstract class Payload;

    /// <summary>
    /// Payload class for image data with a string image identifier.
    /// </summary>
    private class ImagePayload : Payload
    {
        /// <summary>
        /// Gets the image identifier.
        /// </summary>
        public string Image { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePayload"/> class.
        /// </summary>
        /// <param name="image">The image identifier.</param>
        public ImagePayload(string image)
        {
            Image = image;
        }
    }

    /// <summary>
    /// Payload class for link data with a string link.
    /// </summary>
    private class LinkPayload : Payload
    {
        /// <summary>
        /// Gets or sets the link.
        /// </summary>
        public string Link { get; set; } = string.Empty;
    }

    /// <summary>
    /// Base class for model types in the target hierarchy.
    /// </summary>
    private abstract class Model;

    /// <summary>
    /// Model class for image data with a string image identifier.
    /// </summary>
    private class ImageModel : Model
    {
        /// <summary>
        /// Gets or sets the image identifier.
        /// </summary>
        public string Image { get; set; } = string.Empty;
    }

    /// <summary>
    /// Model class for link data with a string link.
    /// </summary>
    private class LinkModel : Model
    {
        /// <summary>
        /// Gets the link.
        /// </summary>
        public string Link { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkModel"/> class.
        /// </summary>
        /// <param name="link">The link.</param>
        public LinkModel(string link)
        {
            Link = link;
        }
    }

    /// <summary>
    /// Base class for request types with a resolution ID.
    /// </summary>
    private abstract class Req
    {
        /// <summary>
        /// Gets the resolution ID.
        /// </summary>
        [ResolutionId]
        public string Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Req"/> class.
        /// </summary>
        /// <param name="type">The resolution ID.</param>
        protected Req(string type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// Request class for image data with a string data identifier.
    /// </summary>
    private class ImageReq : Req
    {
        /// <summary>
        /// Gets the data identifier.
        /// </summary>
        public string Data { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageReq"/> class.
        /// </summary>
        /// <param name="data">The data identifier.</param>
        public ImageReq(string data)
            : base(typeof(ImageMod).GetIdString())
        {
            Data = data;
        }
    }

    /// <summary>
    /// Request class for link data with a string data identifier.
    /// </summary>
    private class LinkReq : Req
    {
        /// <summary>
        /// Gets or sets the data identifier.
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkReq"/> class.
        /// </summary>
        public LinkReq()
            : base(typeof(LinkMod).GetIdString()) { }
    }

    /// <summary>
    /// Base class for model types with a resolution ID.
    /// </summary>
    private abstract class Mod
    {
        /// <summary>
        /// Gets the resolution ID.
        /// </summary>
        [ResolutionId]
        public string Type => GetType().GetIdString();
    }

    /// <summary>
    /// Model class for image data with a string data identifier.
    /// </summary>
    private class ImageMod : Mod
    {
        /// <summary>
        /// Gets or sets the data identifier.
        /// </summary>
        public string Data { get; set; } = string.Empty;
    }

    /// <summary>
    /// Model class for link data with a string data identifier.
    /// </summary>
    private class LinkMod : Mod
    {
        /// <summary>
        /// Gets the data identifier.
        /// </summary>
        public string Data { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkMod"/> class.
        /// </summary>
        /// <param name="data">The data identifier.</param>
        public LinkMod(string data)
        {
            Data = data;
        }
    }
}
