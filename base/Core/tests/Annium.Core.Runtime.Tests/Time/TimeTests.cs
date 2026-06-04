using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Core.Runtime.Time;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Core.Runtime.Tests.Time;

/// <summary>
/// Tests for the time configuration builder terminator guard.
/// </summary>
public class TimeConfigurationBuilderTests
{
    /// <summary>
    /// SetDefault without any prior WithXxxTime call throws InvalidOperationException
    /// (no provider was registered to make default).
    /// </summary>
    [Fact]
    public void SetDefault_WithoutProvider_ThrowsInvalidOperationException()
    {
        // arrange
        var container = new ServiceContainer();
        var builder = container.AddTime();

        // assert
        Wrap.It(() => builder.SetDefault()).Throws<InvalidOperationException>();
    }
}

/// <summary>
/// Tests for the ManagedTimeProvider behaviour accessed via ITimeManager.
/// All tests use managed time exclusively so there are no wall-clock dependencies.
/// </summary>
public class ManagedTimeProviderTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedTimeProviderTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ManagedTimeProviderTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        // TestBase.SharedRegister already calls WithManagedTime().WithRelativeTime().SetDefault()
        // so ITimeManager and ITimeProviderSwitcher are always available.
        // We switch the active provider to managed time so that ITimeProvider reflects it.
    }

    /// <summary>
    /// SetNow advances ITimeProvider.Now when the active provider is switched to managed time.
    /// </summary>
    [Fact]
    public void SetNow_ForwardAdvance_UpdatesNow()
    {
        // arrange
        var manager = Get<ITimeManager>();
        var switcher = Get<ITimeProviderSwitcher>();
        var provider = Get<ITimeProvider>();
        switcher.UseManagedTime();

        var t0 = Instant.FromUnixTimeSeconds(1_000_000);
        var t1 = t0 + Duration.FromSeconds(30);

        // act
        manager.SetNow(t0);
        var after0 = provider.Now;
        manager.SetNow(t1);
        var after1 = provider.Now;

        // assert
        after0.Is(t0);
        after1.Is(t1);
    }

    /// <summary>
    /// SetNow keeps DateTimeNow, UnixMsNow, and UnixSecondsNow consistent with Now.
    /// </summary>
    [Fact]
    public void SetNow_Coherence_AllFieldsMatchNow()
    {
        // arrange
        var manager = Get<ITimeManager>();
        var switcher = Get<ITimeProviderSwitcher>();
        var provider = Get<ITimeProvider>();
        switcher.UseManagedTime();

        var instant = Instant.FromUnixTimeSeconds(1_700_000_000L);

        // act
        manager.SetNow(instant);

        // assert
        provider.Now.Is(instant);
        provider.DateTimeNow.Is(instant.ToDateTimeUtc());
        provider.UnixMsNow.Is(instant.ToUnixTimeMilliseconds());
        provider.UnixSecondsNow.Is(instant.ToUnixTimeSeconds());
    }

    /// <summary>
    /// OnNowChanged fires with the positive Duration when time is advanced forward.
    /// </summary>
    [Fact]
    public void OnNowChanged_ForwardAdvance_PositiveDeltaFired()
    {
        // arrange
        var manager = Get<ITimeManager>();
        var t0 = Instant.FromUnixTimeSeconds(500);
        var t1 = t0 + Duration.FromSeconds(10);
        Duration? fired = null;

        manager.SetNow(t0);
        manager.OnNowChanged += d => fired = d;

        // act
        manager.SetNow(t1);

        // assert
        fired.IsNotNull();
        fired!.Value.Is(Duration.FromSeconds(10));
    }

    /// <summary>
    /// OnNowChanged fires with a negative Duration when time is moved backwards.
    /// </summary>
    [Fact]
    public void OnNowChanged_BackwardMove_NegativeDeltaFired()
    {
        // arrange
        var manager = Get<ITimeManager>();
        var t0 = Instant.FromUnixTimeSeconds(500);
        var t1 = t0 - Duration.FromSeconds(5);
        Duration? fired = null;

        manager.SetNow(t0);
        manager.OnNowChanged += d => fired = d;

        // act
        manager.SetNow(t1);

        // assert
        fired.IsNotNull();
        fired!.Value.Is(Duration.FromSeconds(-5));
    }

    /// <summary>
    /// OnNowChanged fires with Duration.Zero when the same instant is set twice.
    /// </summary>
    [Fact]
    public void OnNowChanged_SameInstant_ZeroDeltaFired()
    {
        // arrange
        var manager = Get<ITimeManager>();
        var t0 = Instant.FromUnixTimeSeconds(1_000);
        Duration? fired = null;

        manager.SetNow(t0);
        manager.OnNowChanged += d => fired = d;

        // act
        manager.SetNow(t0);

        // assert
        fired.IsNotNull();
        fired!.Value.Is(Duration.Zero);
    }

    /// <summary>
    /// All subscribers registered to OnNowChanged are notified when SetNow is called.
    /// </summary>
    [Fact]
    public void OnNowChanged_MultipleSubscribers_AllNotified()
    {
        // arrange
        var manager = Get<ITimeManager>();
        var base0 = Instant.FromUnixTimeSeconds(0);
        var notifications = new List<int>();

        manager.SetNow(base0);
        manager.OnNowChanged += _ => notifications.Add(1);
        manager.OnNowChanged += _ => notifications.Add(2);
        manager.OnNowChanged += _ => notifications.Add(3);

        // act
        manager.SetNow(base0 + Duration.FromSeconds(1));

        // assert
        notifications.Has(3);
        notifications.Contains(1).IsTrue();
        notifications.Contains(2).IsTrue();
        notifications.Contains(3).IsTrue();
    }

    /// <summary>
    /// Unsubscribing from OnNowChanged before the next SetNow prevents that subscriber
    /// from receiving further notifications.
    /// </summary>
    [Fact]
    public void OnNowChanged_Unsubscribe_StopsNotifications()
    {
        // arrange
        var manager = Get<ITimeManager>();
        var base0 = Instant.FromUnixTimeSeconds(100);
        var callCount = 0;

        manager.SetNow(base0);

        void Handler(Duration _) => callCount++;
        manager.OnNowChanged += Handler;

        // act — first call: subscriber is active
        manager.SetNow(base0 + Duration.FromSeconds(1));
        // unsubscribe
        manager.OnNowChanged -= Handler;
        // second call: subscriber should not fire
        manager.SetNow(base0 + Duration.FromSeconds(2));

        // assert
        callCount.Is(1);
    }
}

/// <summary>
/// Tests for ITimeProviderSwitcher — verifying that the active provider
/// can be switched at runtime between managed and relative modes.
/// </summary>
public class TimeProviderSwitcherTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeProviderSwitcherTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public TimeProviderSwitcherTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// UseManagedTime causes ITimeProvider.Now to reflect the managed time
    /// after SetNow is called on ITimeManager.
    /// </summary>
    [Fact]
    public void UseManagedTime_AfterSetNow_ProviderReflectsManagedTime()
    {
        // arrange
        var switcher = Get<ITimeProviderSwitcher>();
        var manager = Get<ITimeManager>();
        var provider = Get<ITimeProvider>();

        var knownInstant = Instant.FromUnixTimeSeconds(999_999_999L);
        manager.SetNow(knownInstant);

        // act
        switcher.UseManagedTime();

        // assert — ITimeProvider.Now must match exactly what was set
        provider.Now.Is(knownInstant);
    }

    /// <summary>
    /// After switching to managed time and advancing it, switching to relative time
    /// moves ITimeProvider away from managed time (Now no longer equals managed time value).
    /// </summary>
    [Fact]
    public void UseRelativeTime_AfterManagedTime_ProviderNoLongerReflectsManagedTime()
    {
        // arrange
        var switcher = Get<ITimeProviderSwitcher>();
        var manager = Get<ITimeManager>();
        var provider = Get<ITimeProvider>();

        // set a very specific managed instant far in the future
        var futureInstant = Instant.FromUnixTimeSeconds(9_000_000_000L);
        manager.SetNow(futureInstant);
        switcher.UseManagedTime();
        provider.Now.Is(futureInstant); // sanity

        // act
        switcher.UseRelativeTime();

        // assert — relative provider anchors at BclEpoch, so Now should NOT equal the managed instant
        (provider.Now == futureInstant).IsFalse();
    }

    /// <summary>
    /// UseRealTime throws because real time was not registered in the default test container.
    /// SharedRegister only registers managed + relative time.
    /// </summary>
    [Fact]
    public void UseRealTime_NotRegistered_Throws()
    {
        // arrange
        var switcher = Get<ITimeProviderSwitcher>();

        // assert
        Wrap.It(() => switcher.UseRealTime()).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Switching between managed and relative time multiple times keeps ITimeProvider coherent.
    /// </summary>
    [Fact]
    public void SwitchBetweenModes_Repeatedly_ProviderCoherent()
    {
        // arrange
        var switcher = Get<ITimeProviderSwitcher>();
        var manager = Get<ITimeManager>();
        var provider = Get<ITimeProvider>();

        var t1 = Instant.FromUnixTimeSeconds(1_000_000L);
        var t2 = Instant.FromUnixTimeSeconds(2_000_000L);

        manager.SetNow(t1);

        // act + assert round 1: switch to managed
        switcher.UseManagedTime();
        provider.Now.Is(t1);

        // advance managed time
        manager.SetNow(t2);
        provider.Now.Is(t2);

        // switch to relative — now is near epoch, far from t2
        switcher.UseRelativeTime();
        (provider.Now == t2).IsFalse();

        // switch back to managed — see t2 again
        switcher.UseManagedTime();
        provider.Now.Is(t2);
    }
}

/// <summary>
/// Tests for ITimeProvider when the RelativeTimeProvider is active.
/// RelativeTimeProvider anchors at NodaConstants.BclEpoch and advances
/// with wall-clock time — so its value is near epoch (not the current year).
/// </summary>
public class RelativeTimeProviderTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelativeTimeProviderTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public RelativeTimeProviderTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// After switching to relative time, ITimeProvider.Now starts near NodaConstants.BclEpoch
    /// (0001-01-01 UTC), not near the current date.
    /// </summary>
    [Fact]
    public void Now_WithRelativeTime_IsNearBclEpoch()
    {
        // arrange
        var switcher = Get<ITimeProviderSwitcher>();
        var provider = Get<ITimeProvider>();
        switcher.UseRelativeTime();

        // act
        var now = provider.Now;

        // assert — must be within a small tolerance of BclEpoch (the test runs quickly)
        var delta = now - NodaConstants.BclEpoch;
        (delta >= Duration.Zero).IsTrue();
        (delta < Duration.FromSeconds(60)).IsTrue(); // generous upper bound for slow CI
    }

    /// <summary>
    /// After switching to relative time, DateTimeNow is coherent with ITimeProvider.Now.
    /// </summary>
    [Fact]
    public void DateTimeNow_WithRelativeTime_CoherentWithNow()
    {
        // arrange
        var switcher = Get<ITimeProviderSwitcher>();
        var provider = Get<ITimeProvider>();
        switcher.UseRelativeTime();

        // act — capture both properties in close succession
        var now = provider.Now;
        var dtNow = provider.DateTimeNow;

        // assert — DateTimeNow must correspond to an instant no earlier than now
        var fromDt = Instant.FromDateTimeUtc(dtNow);
        (fromDt >= now).IsTrue();
        // and not more than a second later (both captured on same tick)
        (fromDt - now < Duration.FromSeconds(1)).IsTrue();
    }

    /// <summary>
    /// After switching to relative time, UnixMsNow is coherent with ITimeProvider.Now.
    /// </summary>
    [Fact]
    public void UnixMsNow_WithRelativeTime_CoherentWithNow()
    {
        // arrange
        var switcher = Get<ITimeProviderSwitcher>();
        var provider = Get<ITimeProvider>();
        switcher.UseRelativeTime();

        // act
        var now = provider.Now;
        var ms = provider.UnixMsNow;

        // assert — ms tracks now's millisecond epoch. now and ms are two separate wall-clock reads
        // under relative time, so allow a generous window (a GC/scheduling stall between the reads
        // can exceed a tight tick tolerance). Exact-coherence of the derived properties is proven
        // deterministically under managed time (see SetNow_*_KeepsDerivedPropertiesCoherent).
        var diff = Math.Abs(ms - now.ToUnixTimeMilliseconds());
        (diff <= 1000).IsTrue();
    }

    /// <summary>
    /// After switching to relative time, UnixSecondsNow is coherent with ITimeProvider.Now.
    /// </summary>
    [Fact]
    public void UnixSecondsNow_WithRelativeTime_CoherentWithNow()
    {
        // arrange
        var switcher = Get<ITimeProviderSwitcher>();
        var provider = Get<ITimeProvider>();
        switcher.UseRelativeTime();

        // act
        var now = provider.Now;
        var sec = provider.UnixSecondsNow;

        // assert — seconds value must be within 1 second of now (rounding)
        var diff = Math.Abs(sec - now.ToUnixTimeSeconds());
        (diff <= 1).IsTrue();
    }
}

/// <summary>
/// Tests for ITimeManager.Now — directly via the manager interface,
/// without switching the active ITimeProvider.
/// </summary>
public class TimeManagerNowTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeManagerNowTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public TimeManagerNowTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// ITimeManager.Now returns the instant most recently set via SetNow.
    /// </summary>
    [Fact]
    public void Now_AfterSetNow_ReturnsSetInstant()
    {
        // arrange
        var manager = Get<ITimeManager>();
        var instant = Instant.FromUnixTimeSeconds(123_456_789L);

        // act
        manager.SetNow(instant);

        // assert
        manager.Now.Is(instant);
    }

    /// <summary>
    /// Calling SetNow multiple times keeps ITimeManager.Now at the last-set value.
    /// </summary>
    [Fact]
    public void Now_MultipleSetNow_ReturnsLastInstant()
    {
        // arrange
        var manager = Get<ITimeManager>();
        var t1 = Instant.FromUnixTimeSeconds(100L);
        var t2 = Instant.FromUnixTimeSeconds(200L);
        var t3 = Instant.FromUnixTimeSeconds(150L); // even backward is accepted

        // act
        manager.SetNow(t1);
        manager.SetNow(t2);
        manager.SetNow(t3);

        // assert
        manager.Now.Is(t3);
    }

    /// <summary>
    /// The default value of ITimeManager.Now is the NodaTime Instant default (Unix epoch).
    /// </summary>
    [Fact]
    public void Now_BeforeSetNow_IsDefaultInstant()
    {
        // arrange
        var manager = Get<ITimeManager>();

        // assert — Instant default is Instant.FromUnixTimeSeconds(0) (1970-01-01)
        manager.Now.Is(default);
    }
}

/// <summary>
/// Tests for <see cref="ManagedTimeProviderExtensions"/> — each of the eight Add* convenience
/// methods (AddSecond, AddSeconds, AddMinute, AddMinutes, AddHour, AddHours, AddDay, AddDays).
/// All assertions are deterministic: a known instant is set first; the extension is called;
/// the resulting <see cref="ITimeManager.Now"/> is compared to the expected advance.
/// No wall-clock time or Task.Delay is used.
/// </summary>
public class ManagedTimeProviderExtensionsTests : TestBase
{
    /// <summary>
    /// A fixed reference instant used as the starting point for every test in this class.
    /// </summary>
    private static readonly Instant _baseInstant = Instant.FromUnixTimeSeconds(1_000_000L);

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedTimeProviderExtensionsTests"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public ManagedTimeProviderExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// AddSecond advances ITimeManager.Now by exactly one second.
    /// </summary>
    [Fact]
    public void AddSecond_AdvancesNowByOneSecond()
    {
        // arrange
        var manager = Get<ITimeManager>();
        manager.SetNow(_baseInstant);

        // act
        manager.AddSecond();

        // assert
        manager.Now.Is(_baseInstant + Duration.FromSeconds(1));
    }

    /// <summary>
    /// AddSeconds(n) advances ITimeManager.Now by exactly n seconds.
    /// </summary>
    [Fact]
    public void AddSeconds_AdvancesNowByGivenSeconds()
    {
        // arrange
        var manager = Get<ITimeManager>();
        manager.SetNow(_baseInstant);
        const long n = 45L;

        // act
        manager.AddSeconds(n);

        // assert
        manager.Now.Is(_baseInstant + Duration.FromSeconds(n));
    }

    /// <summary>
    /// AddMinute advances ITimeManager.Now by exactly sixty seconds.
    /// </summary>
    [Fact]
    public void AddMinute_AdvancesNowByOneMinute()
    {
        // arrange
        var manager = Get<ITimeManager>();
        manager.SetNow(_baseInstant);

        // act
        manager.AddMinute();

        // assert
        manager.Now.Is(_baseInstant + Duration.FromSeconds(60));
    }

    /// <summary>
    /// AddMinutes(n) advances ITimeManager.Now by exactly n*60 seconds.
    /// </summary>
    [Fact]
    public void AddMinutes_AdvancesNowByGivenMinutes()
    {
        // arrange
        var manager = Get<ITimeManager>();
        manager.SetNow(_baseInstant);
        const long n = 3L;

        // act
        manager.AddMinutes(n);

        // assert
        manager.Now.Is(_baseInstant + Duration.FromSeconds(n * 60));
    }

    /// <summary>
    /// AddHour advances ITimeManager.Now by exactly 3600 seconds.
    /// </summary>
    [Fact]
    public void AddHour_AdvancesNowByOneHour()
    {
        // arrange
        var manager = Get<ITimeManager>();
        manager.SetNow(_baseInstant);

        // act
        manager.AddHour();

        // assert
        manager.Now.Is(_baseInstant + Duration.FromSeconds(3600));
    }

    /// <summary>
    /// AddHours(n) advances ITimeManager.Now by exactly n*3600 seconds.
    /// </summary>
    [Fact]
    public void AddHours_AdvancesNowByGivenHours()
    {
        // arrange
        var manager = Get<ITimeManager>();
        manager.SetNow(_baseInstant);
        const long n = 2L;

        // act
        manager.AddHours(n);

        // assert
        manager.Now.Is(_baseInstant + Duration.FromSeconds(n * 3600));
    }

    /// <summary>
    /// AddDay advances ITimeManager.Now by exactly 86400 seconds.
    /// </summary>
    [Fact]
    public void AddDay_AdvancesNowByOneDay()
    {
        // arrange
        var manager = Get<ITimeManager>();
        manager.SetNow(_baseInstant);

        // act
        manager.AddDay();

        // assert
        manager.Now.Is(_baseInstant + Duration.FromSeconds(86_400));
    }

    /// <summary>
    /// AddDays(n) advances ITimeManager.Now by exactly n*86400 seconds.
    /// </summary>
    [Fact]
    public void AddDays_AdvancesNowByGivenDays()
    {
        // arrange
        var manager = Get<ITimeManager>();
        manager.SetNow(_baseInstant);
        const long n = 7L;

        // act
        manager.AddDays(n);

        // assert
        manager.Now.Is(_baseInstant + Duration.FromSeconds(n * 86_400));
    }
}
