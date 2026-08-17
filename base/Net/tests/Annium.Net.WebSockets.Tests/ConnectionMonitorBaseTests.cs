using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Net.WebSockets.Tests;

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
    public void Stop_CalledTwice_HandleStopInvokedOnce()
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
    public void Stop_WithoutStart_DoesNothing()
    {
        var monitor = new CountingConnectionMonitor(VoidLogger.Instance);

        monitor.Stop();

        monitor.StopCount.Is(0);
    }

    /// <summary>
    /// Concrete <see cref="ConnectionMonitorBase"/> subclass that counts how many times
    /// <c>HandleStart</c> and <c>HandleStop</c> are invoked.
    /// </summary>
    /// <param name="logger">Logger used for tracing.</param>
    private sealed class CountingConnectionMonitor(ILogger logger) : ConnectionMonitorBase(logger)
    {
        /// <summary>Number of times <c>HandleStart</c> was called.</summary>
        public int StartCount { get; private set; }

        /// <summary>Number of times <c>HandleStop</c> was called.</summary>
        public int StopCount { get; private set; }

        /// <summary>Increments <c>StartCount</c>; verifies the base class invokes the hook exactly once per start cycle.</summary>
        protected override void HandleStart() => StartCount++;

        /// <summary>Increments <c>StopCount</c>; verifies the base class invokes the hook exactly once per stop cycle.</summary>
        protected override void HandleStop() => StopCount++;
    }
}
