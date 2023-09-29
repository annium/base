using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Mapper.Tests.Resolvers;

public class ResolutionMapResolverTest : TestBase
{
    public ResolutionMapResolverTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    [Fact]
    public void SignatureResolution_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Payload[] { new ImagePayload("img"), new LinkPayload { Link = "lnk" } };

        // act
        var result = mapper.Map<List<Model>>(value);

        // assert
        result.Has(2);
        result.At(0).As<ImageModel>().Image.Is("img");
        result.At(1).As<LinkModel>().Link.Is("lnk");
    }

    [Fact]
    public void IdResolution_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var value = new Req[] { new ImageReq("img"), new LinkReq { Data = "lnk" } };

        // act
        var result = mapper.Map<List<Mod>>(value);

        // assert
        result.Has(2);
        result.At(0).As<ImageMod>().Data.Is("img");
        result.At(1).As<LinkMod>().Data.Is("lnk");
    }

    private abstract class Payload
    {
    }

    private class ImagePayload : Payload
    {
        public string Image { get; }

        public ImagePayload(string image)
        {
            Image = image;
        }
    }

    private class LinkPayload : Payload
    {
        public string Link { get; set; } = string.Empty;
    }

    private abstract class Model
    {
    }

    private class ImageModel : Model
    {
        public string Image { get; set; } = string.Empty;
    }

    private class LinkModel : Model
    {
        public string Link { get; }

        public LinkModel(string link)
        {
            Link = link;
        }
    }

    private abstract class Req
    {
        [ResolutionId]
        public string Type { get; }

        protected Req(string type)
        {
            Type = type;
        }
    }

    private class ImageReq : Req
    {
        public string Data { get; }

        public ImageReq(string data) : base(typeof(ImageMod).GetIdString())
        {
            Data = data;
        }
    }

    private class LinkReq : Req
    {
        public string Data { get; set; } = string.Empty;

        public LinkReq() : base(typeof(LinkMod).GetIdString())
        {
        }
    }

    private abstract class Mod
    {
        [ResolutionId]
        public string Type => GetType().GetIdString();
    }

    private class ImageMod : Mod
    {
        public string Data { get; set; } = string.Empty;
    }

    private class LinkMod : Mod
    {
        public string Data { get; }

        public LinkMod(string data)
        {
            Data = data;
        }
    }
}