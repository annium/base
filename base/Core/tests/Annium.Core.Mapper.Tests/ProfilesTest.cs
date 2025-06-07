using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Core.Mapper.Tests;

/// <summary>
/// Tests for profile-based mapping in the mapper.
/// </summary>
/// <remarks>
/// Verifies that the mapper can:
/// - Map objects using profiles
/// - Handle profile-based mapping rules
/// - Preserve values during mapping
/// - Apply different mapping rules based on profiles
/// </remarks>
public class ProfilesTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfilesTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    /// <remarks>
    /// Registers the mapper with a profile that defines mapping rules.
    /// </remarks>
    public ProfilesTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false).AddProfile(ConfigureProfile));
    }

    /// <summary>
    /// Tests that mapping with a profile works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies that:
    /// - Objects can be mapped using profiles
    /// - Profile-based mapping rules are applied
    /// - The mapping preserves the original values
    /// - The result is a valid instance of the target type
    /// </remarks>
    [Fact]
    public void ConfigurationMapping_Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var date = new DateTime(2000, 10, 7).ToUniversalTime();
        var instant = Instant.FromDateTimeUtc(new DateTime(2002, 6, 17).ToUniversalTime());
        var value = new Payload[]
        {
            new ImagePayload("img", date),
            new LinkPayload { Link = "lnk", Created = instant },
        };

        // act
        var result = mapper.Map<List<Model>>(value);

        // assert
        result.Has(2);
        result.At(0).As<ImageModel>().Image.Is("img");
        result.At(0).As<ImageModel>().Created.ToDateTimeUtc().Is(date);
        result.At(1).As<LinkModel>().Link.Is("lnk");
        result.At(1).As<LinkModel>().Created.Is(instant.ToDateTimeUtc());
    }

    /// <summary>
    /// Configures the mapping profile with DateTime and Instant conversions
    /// </summary>
    /// <param name="p">The profile to configure</param>
    private void ConfigureProfile(Profile p)
    {
        p.Map<DateTime, Instant>(d => Instant.FromDateTimeUtc(d.ToUniversalTime()));
        p.Map<Instant, DateTime>(i => i.ToDateTimeUtc());
    }

    /// <summary>
    /// Abstract model class.
    /// </summary>
    private abstract class Payload;

    /// <summary>
    /// Example LinkPayload class
    /// </summary>
    private class ImagePayload : Payload
    {
        /// <summary>
        /// Image value.
        /// </summary>
        public string Image { get; }

        /// <summary>
        /// Image created date.
        /// </summary>
        public DateTime Created { get; }

        public ImagePayload(string image, DateTime created)
        {
            Image = image;
            Created = created;
        }
    }

    /// <summary>
    /// Example LinkPayload class
    /// </summary>
    private class LinkPayload : Payload
    {
        /// <summary>
        /// Link value.
        /// </summary>
        public string? Link { get; set; }

        /// <summary>
        /// Link created date.
        /// </summary>
        public Instant Created { get; set; }
    }

    /// <summary>
    /// Abstract model class.
    /// </summary>
    private abstract class Model;

    /// <summary>
    /// Example ImageModel class.
    /// </summary>
    private class ImageModel : Model
    {
        /// <summary>
        /// Image value.
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Image created date.
        /// </summary>
        public Instant Created { get; set; }
    }

    /// <summary>
    /// Source class with a nested object.
    /// </summary>
    private class LinkModel : Model
    {
        /// <summary>
        /// Link value.
        /// </summary>
        public string? Link { get; }

        /// <summary>
        /// Link created date.
        /// </summary>
        public DateTime Created { get; }

        public LinkModel(string? link, DateTime created)
        {
            Link = link;
            Created = created;
        }
    }
}
