using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Net.Sockets.Tests.Internal;

/// <summary>
/// Tests for <see cref="Helper.ClassifySendException"/> covering each exception branch in the
/// classifier switch expression.
/// </summary>
public class HelperClassifySendExceptionTests
{
    /// <summary>
    /// A plain subject that implements <see cref="ILogSubject"/> using the void logger so the
    /// classifier can call <c>log.Trace</c> / <c>log.Error</c> without a real logger.
    /// </summary>
    private sealed class LogSubject : ILogSubject
    {
        /// <summary>Gets the void logger instance.</summary>
        public ILogger Logger { get; } = VoidLogger.Instance;
    }

    /// <summary>Log subject passed to <c>ClassifySendException</c> under test.</summary>
    private readonly ILogSubject _log = new LogSubject();

    /// <summary>
    /// An <see cref="OperationCanceledException"/> is classified as Canceled.
    /// </summary>
    [Fact]
    public void ClassifySendException_OperationCanceledException_ReturnsCanceled()
    {
        var result = Helper.ClassifySendException(new OperationCanceledException(), _log);

        result.Is(SocketSendStatus.Canceled);
    }

    /// <summary>
    /// A <see cref="TaskCanceledException"/> (which derives from OCE) is also classified as Canceled.
    /// </summary>
    [Fact]
    public void ClassifySendException_TaskCanceledException_ReturnsCanceled()
    {
        var result = Helper.ClassifySendException(new TaskCanceledException(), _log);

        result.Is(SocketSendStatus.Canceled);
    }

    /// <summary>
    /// An <see cref="ObjectDisposedException"/> is classified as Closed.
    /// </summary>
    [Fact]
    public void ClassifySendException_ObjectDisposedException_ReturnsClosed()
    {
        var result = Helper.ClassifySendException(new ObjectDisposedException("obj"), _log);

        result.Is(SocketSendStatus.Closed);
    }

    /// <summary>
    /// An <see cref="InvalidOperationException"/> (that is not ObjectDisposedException) is
    /// classified as Closed.
    /// </summary>
    [Fact]
    public void ClassifySendException_InvalidOperationException_ReturnsClosed()
    {
        var result = Helper.ClassifySendException(new InvalidOperationException("ioe"), _log);

        result.Is(SocketSendStatus.Closed);
    }

    /// <summary>
    /// An <see cref="IOException"/> whose <see cref="Exception.InnerException"/> is an
    /// <see cref="ObjectDisposedException"/> is classified as Closed.
    /// </summary>
    [Fact]
    public void ClassifySendException_IOExceptionWrappingObjectDisposedException_ReturnsClosed()
    {
        var inner = new ObjectDisposedException("stream");
        var outer = new IOException("io", inner);

        var result = Helper.ClassifySendException(outer, _log);

        result.Is(SocketSendStatus.Closed);
    }

    /// <summary>
    /// An <see cref="IOException"/> whose <see cref="Exception.InnerException"/> is a
    /// <see cref="SocketException"/> is classified as Closed.
    /// </summary>
    [Fact]
    public void ClassifySendException_IOExceptionWrappingSocketException_ReturnsClosed()
    {
        var inner = new SocketException((int)SocketError.ConnectionReset);
        var outer = new IOException("io", inner);

        var result = Helper.ClassifySendException(outer, _log);

        result.Is(SocketSendStatus.Closed);
    }

    /// <summary>
    /// An unknown / arbitrary exception falls through to the default arm and is classified as Closed.
    /// </summary>
    [Fact]
    public void ClassifySendException_UnknownException_ReturnsClosed()
    {
        var result = Helper.ClassifySendException(new Exception("unknown"), _log);

        result.Is(SocketSendStatus.Closed);
    }

    /// <summary>
    /// A custom exception type that does not match any of the explicit arms is classified as Closed
    /// via the default arm.
    /// </summary>
    [Fact]
    public void ClassifySendException_CustomException_ReturnsClosed()
    {
        var result = Helper.ClassifySendException(new CustomException("custom"), _log);

        result.Is(SocketSendStatus.Closed);
    }

    /// <summary>Test-only exception used to exercise the default classification arm.</summary>
    private sealed class CustomException(string message) : Exception(message);
}
