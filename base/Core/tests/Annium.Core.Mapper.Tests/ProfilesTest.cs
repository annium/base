using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Annium.Testing.Lib;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Core.Mapper.Tests;

public class ProfilesTest : TestBase
{
    public ProfilesTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile(ConfigureProfile));
    }

    [Fact]
    public void ConfigurationMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var date = new DateTime(2000, 10, 7).ToUniversalTime();
        var instant = Instant.FromDateTimeUtc(new DateTime(2002, 6, 17).ToUniversalTime());
        var value = new Payload[] { new ImagePayload("img", date), new LinkPayload { Link = "lnk", Created = instant } };

        // act
        var result = mapper.Map<List<Model>>(value);

        // assert
        result.Has(2);
        result.At(0).As<ImageModel>().Image.Is("img");
        result.At(0).As<ImageModel>().Created.ToDateTimeUtc().Is(date);
        result.At(1).As<LinkModel>().Link.Is("lnk");
        result.At(1).As<LinkModel>().Created.Is(instant.ToDateTimeUtc());
    }

    private void ConfigureProfile(Profile p)
    {
        p.Map<DateTime, Instant>(d => Instant.FromDateTimeUtc(d.ToUniversalTime()));
        p.Map<Instant, DateTime>(i => i.ToDateTimeUtc());
    }

    private abstract class Payload
    {
    }

    private class ImagePayload : Payload
    {
        public string Image { get; }

        public DateTime Created { get; }

        public ImagePayload(string image, DateTime created)
        {
            Image = image;
            Created = created;
        }
    }

    private class LinkPayload : Payload
    {
        public string? Link { get; set; }

        public Instant Created { get; set; }
    }

    private abstract class Model
    {
    }

    private class ImageModel : Model
    {
        public string? Image { get; set; }

        public Instant Created { get; set; }
    }

    private class LinkModel : Model
    {
        public string? Link { get; }

        public DateTime Created { get; }

        public LinkModel(string? link, DateTime created)
        {
            Link = link;
            Created = created;
        }
    }
}