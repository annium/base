using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests;

/// <summary>
/// Tests for the Start/Stop idempotency contract of <see cref="ConnectionMonitorBase"/>.
/// </summary>
public class ConnectionMonitorBaseTests
{
    /// <summary>
    /// Calling Start() twice invokes HandleStart exactly once.
    /// </summary>
    [Fact]
    public void Start_CalledTwice_HandleStartInvokedOnce()
    {
        var monitor = new CountingConnectionMonitor(VoidLogger.Instance);

        monitor.Start();
        monitor.Start();

        monitor.StartCount.Is(1);
    }

    /// <summary>
    /// Calling Stop() twice (after one Start) invokes HandleStop exactly once.
    /// </summary>
    [Fact]
    public void Stop_CalledTwiceAfterStart_HandleStopInvokedOnce()
    {
        var monitor = new CountingConnectionMonitor(VoidLogger.Instance);

        monitor.Start();
        monitor.Stop();
        monitor.Stop();

        monitor.StopCount.Is(1);
    }

    /// <summary>
    /// Calling Stop() on a fresh never-started monitor does not invoke HandleStop.
    /// </summary>
    [Fact]
    public void Stop_CalledWithoutStart_HandleStopNotInvoked()
    {
        var monitor = new CountingConnectionMonitor(VoidLogger.Instance);

        monitor.Stop();

        monitor.StopCount.Is(0);
    }

    /// <summary>
    /// Start → Stop → Start sequence calls each handler once per cycle.
    /// </summary>
    [Fact]
    public void StartStop_Cycle_EachHandlerCalledOncePerCycle()
    {
        var monitor = new CountingConnectionMonitor(VoidLogger.Instance);

        monitor.Start();
        monitor.Stop();
        monitor.Start();
        monitor.Stop();

        monitor.StartCount.Is(2);
        monitor.StopCount.Is(2);
    }

    /// <summary>
    /// IsRunning is true between Start and Stop.
    /// </summary>
    [Fact]
    public void IsRunning_TrueAfterStartFalseAfterStop()
    {
        var monitor = new CountingConnectionMonitor(VoidLogger.Instance);

        monitor.IsRunningPublic.IsFalse();

        monitor.Start();
        monitor.IsRunningPublic.IsTrue();

        monitor.Stop();
        monitor.IsRunningPublic.IsFalse();
    }

    /// <summary>
    /// Concrete <see cref="ConnectionMonitorBase"/> subclass that counts how many times
    /// <see cref="HandleStart"/> and <see cref="HandleStop"/> are invoked and exposes
    /// the protected <c>IsRunning</c> property for assertion.
    /// </summary>
    /// <param name="logger">Logger used for tracing.</param>
    private sealed class CountingConnectionMonitor(ILogger logger) : ConnectionMonitorBase(logger)
    {
        /// <summary>Number of times <see cref="HandleStart"/> was called.</summary>
        public int StartCount { get; private set; }

        /// <summary>Number of times <see cref="HandleStop"/> was called.</summary>
        public int StopCount { get; private set; }

        /// <summary>Exposes the protected <c>IsRunning</c> property for tests.</summary>
        public bool IsRunningPublic => IsRunning;

        /// <summary>Increments <c>StartCount</c>; used to assert that the base class invokes the hook exactly once per start cycle regardless of how many times <c>Start()</c> is called.</summary>
        protected override void HandleStart() => StartCount++;

        /// <summary>Increments <c>StopCount</c>; used to assert that the base class invokes the hook exactly once per stop cycle regardless of how many times <c>Stop()</c> is called.</summary>
        protected override void HandleStop() => StopCount++;
    }
}
