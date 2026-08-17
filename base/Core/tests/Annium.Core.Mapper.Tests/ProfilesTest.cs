using System;
using System.Collections.Generic;
using System.Globalization;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePayload"/> class.
        /// </summary>
        /// <param name="image">Image reference carried by the payload.</param>
        /// <param name="created">Creation timestamp.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkModel"/> class.
        /// </summary>
        /// <param name="link">Link the image maps to, if any.</param>
        /// <param name="created">Creation timestamp.</param>
        public LinkModel(string? link, DateTime created)
        {
            Link = link;
            Created = created;
        }
    }
}

/// <summary>
/// Tests DefaultProfile's string↔Uri and string↔Guid round-trip conversions.
/// <c>AddMapper(autoload: false)</c> includes <c>DefaultProfile</c> as a built-in.
/// </summary>
public class DefaultProfileStringConversionsTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultProfileStringConversionsTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public DefaultProfileStringConversionsTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Verifies string→Uri and Uri→string conversions registered in DefaultProfile.
    /// </summary>
    [Fact]
    public void StringUri_RoundTrip_Works()
    {
        var mapper = Get<IMapper>();

        var uri = mapper.Map<Uri>("https://example.com/path");
        uri.ToString().Is("https://example.com/path");

        var back = mapper.Map<string>(uri);
        back.Is("https://example.com/path");
    }

    /// <summary>
    /// Verifies string→Guid and Guid→string conversions registered in DefaultProfile.
    /// </summary>
    [Fact]
    public void StringGuid_RoundTrip_Works()
    {
        var mapper = Get<IMapper>();
        var raw = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

        var guid = mapper.Map<Guid>(raw);
        guid.Is(Guid.Parse(raw));

        var back = mapper.Map<string>(guid);
        back.Is(guid.ToString());
    }
}

/// <summary>
/// Tests DefaultProfile's NodaTime conversions from/to built-in date/time types and strings.
/// <c>AddMapper(autoload: false)</c> includes <c>DefaultProfile</c> as a built-in.
/// </summary>
public class DefaultProfileNodaTimeConversionsTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultProfileNodaTimeConversionsTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public DefaultProfileNodaTimeConversionsTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Verifies string→Instant (Unix-millisecond string) and Instant→string conversions.
    /// DefaultProfile maps string→Instant via <c>Instant.FromUnixTimeMilliseconds(long.Parse(x))</c>.
    /// </summary>
    [Fact]
    public void StringInstant_RoundTrip_Works()
    {
        var mapper = Get<IMapper>();
        var millis = 1_000_000_000_000L; // 2001-09-09 in Unix ms
        var expected = Instant.FromUnixTimeMilliseconds(millis);

        var instant = mapper.Map<Instant>(millis.ToString());
        instant.Is(expected);

        var back = mapper.Map<string>(instant);
        back.Is(expected.ToString());
    }

    /// <summary>
    /// Verifies string→Duration conversion.
    /// DefaultProfile maps string→Duration via <c>Duration.FromTimeSpan(TimeSpan.Parse(x))</c>.
    /// </summary>
    [Fact]
    public void StringDuration_Works()
    {
        var mapper = Get<IMapper>();
        var span = TimeSpan.FromHours(2.5);
        var expected = Duration.FromTimeSpan(span);

        var duration = mapper.Map<Duration>(span.ToString());
        duration.Is(expected);
    }

    /// <summary>
    /// Verifies string→IsoDayOfWeek conversion (parsed by name).
    /// </summary>
    [Fact]
    public void StringIsoDayOfWeek_Works()
    {
        var mapper = Get<IMapper>();

        var day = mapper.Map<IsoDayOfWeek>("Monday");
        day.Is(IsoDayOfWeek.Monday);
    }

    /// <summary>
    /// Verifies DateTime→Instant and Instant→DateTime conversions.
    /// </summary>
    [Fact]
    public void DateTimeInstant_RoundTrip_Works()
    {
        var mapper = Get<IMapper>();
        var dt = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var expected = Instant.FromDateTimeUtc(dt);

        var instant = mapper.Map<Instant>(dt);
        instant.Is(expected);

        var back = mapper.Map<DateTime>(instant);
        back.Is(expected.ToDateTimeUtc());
    }

    /// <summary>
    /// Verifies DateTimeOffset→Instant and Instant→DateTimeOffset conversions.
    /// </summary>
    [Fact]
    public void DateTimeOffsetInstant_RoundTrip_Works()
    {
        var mapper = Get<IMapper>();
        var dto = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var expected = Instant.FromDateTimeOffset(dto);

        var instant = mapper.Map<Instant>(dto);
        instant.Is(expected);

        var back = mapper.Map<DateTimeOffset>(instant);
        back.Is(expected.ToDateTimeOffset());
    }

    /// <summary>
    /// Verifies DateOnly↔LocalDate round-trip conversions.
    /// </summary>
    [Fact]
    public void DateOnlyLocalDate_RoundTrip_Works()
    {
        var mapper = Get<IMapper>();
        var dateOnly = new DateOnly(2024, 3, 20);
        var expected = new LocalDate(2024, 3, 20);

        var localDate = mapper.Map<LocalDate>(dateOnly);
        localDate.Is(expected);

        var back = mapper.Map<DateOnly>(localDate);
        back.Is(dateOnly);
    }

    /// <summary>
    /// Verifies TimeOnly↔LocalTime round-trip conversions.
    /// </summary>
    [Fact]
    public void TimeOnlyLocalTime_RoundTrip_Works()
    {
        var mapper = Get<IMapper>();
        var timeOnly = new TimeOnly(14, 30, 45, 100);
        var expected = new LocalTime(14, 30, 45, 100);

        var localTime = mapper.Map<LocalTime>(timeOnly);
        localTime.Is(expected);

        var back = mapper.Map<TimeOnly>(localTime);
        back.Is(timeOnly);
    }

    /// <summary>
    /// Verifies string→LocalDate conversion (via contextual DateOnly.Parse → LocalDate path).
    /// </summary>
    [Fact]
    public void StringLocalDate_Works()
    {
        var mapper = Get<IMapper>();
        var expected = new LocalDate(2024, 3, 20);

        var localDate = mapper.Map<LocalDate>("2024-03-20");
        localDate.Is(expected);
    }

    /// <summary>
    /// Verifies string→LocalTime conversion (via contextual TimeOnly.Parse → LocalTime path).
    /// </summary>
    [Fact]
    public void StringLocalTime_Works()
    {
        var mapper = Get<IMapper>();
        var expected = new LocalTime(14, 30, 45);

        var localTime = mapper.Map<LocalTime>("14:30:45");
        localTime.Is(expected);
    }
}

/// <summary>
/// Tests DefaultProfile's built-in string→date/time direct parse conversions
/// (DateTime, DateTimeOffset, DateOnly, TimeOnly, TimeSpan — all InvariantCulture).
/// These are distinct from the NodaTime conversions tested in <see cref="DefaultProfileNodaTimeConversionsTest"/>.
/// </summary>
public class DefaultProfileStringDateTimeConversionsTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultProfileStringDateTimeConversionsTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public DefaultProfileStringDateTimeConversionsTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(c => c.AddMapper(autoload: false));
    }

    /// <summary>
    /// Verifies string→DateTime conversion using InvariantCulture parsing.
    /// </summary>
    [Fact]
    public void StringToDateTime_Works()
    {
        var mapper = Get<IMapper>();
        const string s = "2024-06-15T12:00:00";
        var expected = DateTime.Parse(s, CultureInfo.InvariantCulture);

        var result = mapper.Map<DateTime>(s);
        result.Is(expected);
    }

    /// <summary>
    /// Verifies string→DateTimeOffset conversion using InvariantCulture parsing.
    /// </summary>
    [Fact]
    public void StringToDateTimeOffset_Works()
    {
        var mapper = Get<IMapper>();
        const string s = "2024-06-15T12:00:00+02:00";
        var expected = DateTimeOffset.Parse(s, CultureInfo.InvariantCulture);

        var result = mapper.Map<DateTimeOffset>(s);
        result.Is(expected);
    }

    /// <summary>
    /// Verifies string→DateOnly conversion using InvariantCulture parsing.
    /// </summary>
    [Fact]
    public void StringToDateOnly_Works()
    {
        var mapper = Get<IMapper>();
        const string s = "2024-03-20";
        var expected = DateOnly.Parse(s, CultureInfo.InvariantCulture);

        var result = mapper.Map<DateOnly>(s);
        result.Is(expected);
    }

    /// <summary>
    /// Verifies string→TimeOnly conversion using InvariantCulture parsing.
    /// </summary>
    [Fact]
    public void StringToTimeOnly_Works()
    {
        var mapper = Get<IMapper>();
        const string s = "14:30:45";
        var expected = TimeOnly.Parse(s, CultureInfo.InvariantCulture);

        var result = mapper.Map<TimeOnly>(s);
        result.Is(expected);
    }

    /// <summary>
    /// Verifies string→TimeSpan conversion using InvariantCulture parsing.
    /// </summary>
    [Fact]
    public void StringToTimeSpan_Works()
    {
        var mapper = Get<IMapper>();
        const string s = "02:30:00";
        var expected = TimeSpan.Parse(s, CultureInfo.InvariantCulture);

        var result = mapper.Map<TimeSpan>(s);
        result.Is(expected);
    }
}
