using System;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

/// <summary>
/// Time provider that anchors <c>Now</c> at <see cref="NodaConstants.BclEpoch"/> and advances
/// by the elapsed wall-clock time since the instance was constructed. Immediately after
/// construction <c>Now</c> equals epoch (≈ 0001-01-01); after one second it equals epoch + 1s.
/// <para>
/// This is intentionally distinct from a system-clock provider — it gives a deterministic
/// time anchor independent of when the test/replay/simulation runs. Use cases:
/// </para>
/// <list type="bullet">
///   <item><description>Deterministic tests — cancel a token at "Now + 5s" without coupling to wall-clock.</description></item>
///   <item><description>Replay scenarios — events recorded with absolute timestamps replay against epoch-anchored time.</description></item>
///   <item><description>Simulations — simulated systems advance from a reproducible start moment.</description></item>
/// </list>
/// <para>
/// Distinct from <c>ManagedTimeProvider</c>, which exposes a manually-advanced time source for
/// tests that need fine-grained control over time progression. <c>RelativeTimeProvider</c>'s
/// time advances naturally with the wall clock; the only manipulation is the anchor.
/// </para>
/// </summary>
internal class RelativeTimeProvider : IInternalTimeProvider
{
    /// <summary>
    /// The system clock instance
    /// </summary>
    private readonly SystemClock _clock = SystemClock.Instance;

    /// <summary>
    /// Initializes a new instance of RelativeTimeProvider with current time as reference
    /// </summary>
    public RelativeTimeProvider()
    {
        Now = _clock.GetCurrentInstant();
    }

    /// <summary>
    /// Current instant — <see cref="NodaConstants.BclEpoch"/> plus elapsed wall-clock time
    /// since this instance was constructed.
    /// </summary>
    public Instant Now => NodaConstants.BclEpoch + (_clock.GetCurrentInstant() - field);

    /// <summary>
    /// The current date and time as UTC DateTime
    /// </summary>
    public DateTime DateTimeNow => Now.ToDateTimeUtc();

    /// <summary>
    /// The current time as Unix timestamp in milliseconds
    /// </summary>
    public long UnixMsNow => Now.ToUnixTimeMilliseconds();

    /// <summary>
    /// The current time as Unix timestamp in seconds
    /// </summary>
    public long UnixSecondsNow => Now.ToUnixTimeSeconds();
}
