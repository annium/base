using System;
using System.Collections.Generic;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Logging;

/// <summary>
/// Tests that the level-shim extensions on <see cref="ILogSubject"/> route to the correct
/// <see cref="LogLevel"/>. Closes the TG8 gap from review-2026.05.15: a parameter-count mismatch
/// or wrong level constant in any of the sed-generated Debug/Info/Warn shims would silently log
/// at the wrong level — these tests would catch it.
/// </summary>
public class LogSubjectExtensionsLevelTests
{
    /// <summary>
    /// Verifies that calling <c>Trace</c> on a log subject routes the message to <see cref="LogLevel.Trace"/>.
    /// </summary>
    [Fact]
    public void Trace_RoutesToTraceLevel() => RunLevelTest(LogLevel.Trace, subject => subject.Trace("msg"));

    /// <summary>
    /// Verifies that calling <c>Debug</c> on a log subject routes the message to <see cref="LogLevel.Debug"/>.
    /// </summary>
    [Fact]
    public void Debug_RoutesToDebugLevel() => RunLevelTest(LogLevel.Debug, subject => subject.Debug("msg"));

    /// <summary>
    /// Verifies that calling <c>Info</c> on a log subject routes the message to <see cref="LogLevel.Info"/>.
    /// </summary>
    [Fact]
    public void Info_RoutesToInfoLevel() => RunLevelTest(LogLevel.Info, subject => subject.Info("msg"));

    /// <summary>
    /// Verifies that calling <c>Warn</c> on a log subject routes the message to <see cref="LogLevel.Warn"/>.
    /// </summary>
    [Fact]
    public void Warn_RoutesToWarnLevel() => RunLevelTest(LogLevel.Warn, subject => subject.Warn("msg"));

    /// <summary>
    /// Verifies that calling <c>Error</c> on a log subject routes the message to <see cref="LogLevel.Error"/>.
    /// </summary>
    [Fact]
    public void Error_RoutesToErrorLevel() => RunLevelTest(LogLevel.Error, subject => subject.Error("msg"));

    /// <summary>
    /// Verifies that the one-parameter <c>Debug</c> overload forwards the argument to the logger
    /// and records it at <see cref="LogLevel.Debug"/>.
    /// </summary>
    [Fact]
    public void Debug_OneParam_ForwardsArgument()
    {
        var captured = RunCapture(subject => subject.Debug("msg {x}", 42));
        captured.Level.Is(LogLevel.Debug);
        captured.Data.Has(1);
        captured.Data[0].Is(42);
    }

    /// <summary>
    /// Verifies that the one-parameter <c>Info</c> overload forwards the argument to the logger
    /// and records it at <see cref="LogLevel.Info"/>. A non-string argument is used to avoid
    /// resolving to the <c>[CallerFilePath]</c> string overload.
    /// </summary>
    [Fact]
    public void Info_OneParam_ForwardsArgument()
    {
        // non-string T1 to disambiguate from the [CallerFilePath] string overload
        var captured = RunCapture(subject => subject.Info("msg {x}", 99));
        captured.Level.Is(LogLevel.Info);
        captured.Data.Has(1);
        captured.Data[0].Is(99);
    }

    /// <summary>
    /// Verifies that the one-parameter <c>Warn</c> overload forwards the argument to the logger
    /// and records it at <see cref="LogLevel.Warn"/>.
    /// </summary>
    [Fact]
    public void Warn_OneParam_ForwardsArgument()
    {
        var captured = RunCapture(subject => subject.Warn("msg {x}", true));
        captured.Level.Is(LogLevel.Warn);
        captured.Data.Has(1);
        captured.Data[0].Is(true);
    }

    /// <summary>
    /// Verifies that the one-parameter <c>Trace</c> overload forwards the argument to the logger
    /// and records it at <see cref="LogLevel.Trace"/>.
    /// </summary>
    [Fact]
    public void Trace_OneParam_ForwardsArgument()
    {
        var captured = RunCapture(subject => subject.Trace("msg {x}", 7));
        captured.Level.Is(LogLevel.Trace);
        captured.Data.Has(1);
        captured.Data[0].Is(7);
    }

    /// <summary>
    /// Verifies that the one-parameter <c>Error</c> overload forwards the argument to the logger
    /// and records it at <see cref="LogLevel.Error"/>.
    /// </summary>
    [Fact]
    public void Error_OneParam_ForwardsArgument()
    {
        var captured = RunCapture(subject => subject.Error("msg {x}", 13));
        captured.Level.Is(LogLevel.Error);
        captured.Data.Has(1);
        captured.Data[0].Is(13);
    }

    /// <summary>
    /// Verifies that a message logged below the configured global level is not forwarded to the logger.
    /// </summary>
    [Fact]
    public void Log_BelowGlobalLevel_IsNotForwardedToLogger()
    {
        var originalLevel = LogConfig.Level;
        try
        {
            LogConfig.SetLevel(LogLevel.Info);
            var logger = new CapturingLogger();
            var subject = new TestSubject(logger);

            subject.Trace("msg-below");

            logger.Entries.IsEmpty();
        }
        finally
        {
            LogConfig.SetLevel(originalLevel);
        }
    }

    /// <summary>
    /// Verifies that messages at or above the configured global level are forwarded to the logger.
    /// </summary>
    [Fact]
    public void Log_AtOrAboveGlobalLevel_IsForwardedToLogger()
    {
        var originalLevel = LogConfig.Level;
        try
        {
            LogConfig.SetLevel(LogLevel.Info);
            var logger = new CapturingLogger();
            var subject = new TestSubject(logger);

            subject.Info("msg-info");
            subject.Warn("msg-warn");

            logger.Entries.Has(2);
            logger.Entries[0].Level.Is(LogLevel.Info);
            logger.Entries[0].Message.Is("msg-info");
            logger.Entries[1].Level.Is(LogLevel.Warn);
            logger.Entries[1].Message.Is("msg-warn");
        }
        finally
        {
            LogConfig.SetLevel(originalLevel);
        }
    }

    /// <summary>
    /// Two-param overload forwards both parameters in order. A swap to <c>[x2, x1]</c> would
    /// produce wrong structured-log data and is undetectable without this assertion.
    /// </summary>
    [Fact]
    public void Log_TwoParams_BothForwardedInOrder()
    {
        var captured = RunCapture(subject => subject.Log(LogLevel.Info, "msg {x1} {x2}", 1, 2));
        captured.Level.Is(LogLevel.Info);
        captured.Data.Has(2);
        captured.Data[0].Is(1);
        captured.Data[1].Is(2);
    }

    /// <summary>
    /// Three-param overload forwards all three parameters in order.
    /// </summary>
    [Fact]
    public void Log_ThreeParams_AllForwardedInOrder()
    {
        var captured = RunCapture(subject => subject.Log(LogLevel.Info, "msg {x1} {x2} {x3}", 1, 2, 3));
        captured.Data.Has(3);
        captured.Data[0].Is(1);
        captured.Data[1].Is(2);
        captured.Data[2].Is(3);
    }

    /// <summary>
    /// Four-param overload forwards all four parameters in order.
    /// </summary>
    [Fact]
    public void Log_FourParams_AllForwardedInOrder()
    {
        var captured = RunCapture(subject => subject.Log(LogLevel.Info, "msg {x1} {x2} {x3} {x4}", 1, 2, 3, 4));
        captured.Data.Has(4);
        captured.Data[0].Is(1);
        captured.Data[1].Is(2);
        captured.Data[2].Is(3);
        captured.Data[3].Is(4);
    }

    /// <summary>
    /// Five-param overload forwards all five parameters in order.
    /// </summary>
    [Fact]
    public void Log_FiveParams_AllForwardedInOrder()
    {
        var captured = RunCapture(subject => subject.Log(LogLevel.Info, "msg {x1} {x2} {x3} {x4} {x5}", 1, 2, 3, 4, 5));
        captured.Data.Has(5);
        for (var i = 0; i < 5; i++)
            captured.Data[i].Is(i + 1);
    }

    /// <summary>
    /// Six-param overload forwards all six parameters in order.
    /// </summary>
    [Fact]
    public void Log_SixParams_AllForwardedInOrder()
    {
        var captured = RunCapture(subject =>
            subject.Log(LogLevel.Info, "msg {x1} {x2} {x3} {x4} {x5} {x6}", 1, 2, 3, 4, 5, 6)
        );
        captured.Data.Has(6);
        for (var i = 0; i < 6; i++)
            captured.Data[i].Is(i + 1);
    }

    /// <summary>
    /// Seven-param overload forwards all seven parameters in order.
    /// </summary>
    [Fact]
    public void Log_SevenParams_AllForwardedInOrder()
    {
        var captured = RunCapture(subject =>
            subject.Log(LogLevel.Info, "msg {x1} {x2} {x3} {x4} {x5} {x6} {x7}", 1, 2, 3, 4, 5, 6, 7)
        );
        captured.Data.Has(7);
        for (var i = 0; i < 7; i++)
            captured.Data[i].Is(i + 1);
    }

    /// <summary>
    /// Eight-param overload forwards all eight parameters in order. The maximum arity in the
    /// generated Log shim family — guards against an off-by-one in the data-array literal
    /// (<c>[x1, x2, x3, x4, x5, x6, x7, x8]</c>) in any copy of the Log.cs file.
    /// </summary>
    [Fact]
    public void Log_EightParams_AllForwardedInOrder()
    {
        var captured = RunCapture(subject =>
            subject.Log(LogLevel.Info, "msg {x1} {x2} {x3} {x4} {x5} {x6} {x7} {x8}", 1, 2, 3, 4, 5, 6, 7, 8)
        );
        captured.Data.Has(8);
        for (var i = 0; i < 8; i++)
            captured.Data[i].Is(i + 1);
    }

    /// <summary>
    /// Runs the given <paramref name="action"/> against a capturing subject, then asserts the
    /// captured entry was logged at <paramref name="expected"/> level with the literal message "msg".
    /// </summary>
    /// <param name="expected">The log level the extension method is expected to route to.</param>
    /// <param name="action">The logging call under test, expressed as an action on <see cref="ILogSubject"/>.</param>
    private static void RunLevelTest(LogLevel expected, Action<ILogSubject> action)
    {
        var captured = RunCapture(action);
        captured.Level.Is(expected);
        captured.Message.Is("msg");
    }

    /// <summary>
    /// Creates a <see cref="CapturingLogger"/> and a <see cref="TestSubject"/>, runs
    /// <paramref name="action"/>, asserts exactly one entry was captured, and returns it.
    /// </summary>
    /// <param name="action">The logging call under test, expressed as an action on <see cref="ILogSubject"/>.</param>
    /// <returns>The single <see cref="CapturedEntry"/> written by the action.</returns>
    private static CapturedEntry RunCapture(Action<ILogSubject> action)
    {
        var originalLevel = LogConfig.Level;
        try
        {
            LogConfig.SetLevel(LogLevel.Trace);
            var logger = new CapturingLogger();
            var subject = new TestSubject(logger);
            action(subject);
            logger.Entries.Has(1);
            return logger.Entries[0];
        }
        finally
        {
            LogConfig.SetLevel(originalLevel);
        }
    }

    /// <summary>
    /// Represents a single log entry captured by <see cref="CapturingLogger"/>.
    /// </summary>
    private sealed record CapturedEntry(LogLevel Level, string Message, IReadOnlyList<object?> Data);

    /// <summary>
    /// In-memory <see cref="ILogger"/> that accumulates every call to <see cref="Log"/> and
    /// <see cref="Error"/> as <see cref="CapturedEntry"/> records for assertion.
    /// </summary>
    private sealed class CapturingLogger : ILogger
    {
        /// <summary>
        /// Gets the list of entries recorded so far.
        /// </summary>
        public List<CapturedEntry> Entries { get; } = new();

        /// <summary>
        /// Records the structured-log call as a <see cref="CapturedEntry"/> at the given level.
        /// </summary>
        /// <param name="subject">The log subject.</param>
        /// <param name="file">Source file path.</param>
        /// <param name="member">Calling member name.</param>
        /// <param name="line">Source line number.</param>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message template.</param>
        /// <param name="data">Structured-log data arguments.</param>
        public void Log(
            object subject,
            string file,
            string member,
            int line,
            LogLevel level,
            string message,
            IReadOnlyList<object?> data
        ) => Entries.Add(new CapturedEntry(level, message, data));

        /// <summary>
        /// Records the exception call as a <see cref="CapturedEntry"/> at <see cref="LogLevel.Error"/>.
        /// </summary>
        /// <param name="subject">The log subject.</param>
        /// <param name="file">Source file path.</param>
        /// <param name="member">Calling member name.</param>
        /// <param name="line">Source line number.</param>
        /// <param name="ex">The exception to record.</param>
        /// <param name="data">Structured-log data arguments.</param>
        public void Error(
            object subject,
            string file,
            string member,
            int line,
            Exception ex,
            IReadOnlyList<object?> data
        ) => Entries.Add(new CapturedEntry(LogLevel.Error, ex.Message, data));
    }

    /// <summary>
    /// Minimal <see cref="ILogSubject"/> implementation that delegates to an injected
    /// <see cref="ILogger"/> — used to drive the extension-method shims under test.
    /// </summary>
    private sealed class TestSubject : ILogSubject
    {
        public TestSubject(ILogger logger) => Logger = logger;

        /// <summary>
        /// Gets the logger instance used by this subject.
        /// </summary>
        public ILogger Logger { get; }
    }
}
