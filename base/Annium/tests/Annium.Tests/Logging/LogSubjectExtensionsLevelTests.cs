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
    [Fact]
    public void Trace_RoutesToTraceLevel() => RunLevelTest(LogLevel.Trace, subject => subject.Trace("msg"));

    [Fact]
    public void Debug_RoutesToDebugLevel() => RunLevelTest(LogLevel.Debug, subject => subject.Debug("msg"));

    [Fact]
    public void Info_RoutesToInfoLevel() => RunLevelTest(LogLevel.Info, subject => subject.Info("msg"));

    [Fact]
    public void Warn_RoutesToWarnLevel() => RunLevelTest(LogLevel.Warn, subject => subject.Warn("msg"));

    [Fact]
    public void Error_RoutesToErrorLevel() => RunLevelTest(LogLevel.Error, subject => subject.Error("msg"));

    [Fact]
    public void Debug_OneParam_ForwardsArgument()
    {
        var captured = RunCapture(subject => subject.Debug("msg {x}", 42));
        captured.Level.Is(LogLevel.Debug);
        captured.Data.Has(1);
        captured.Data[0].Is(42);
    }

    [Fact]
    public void Info_OneParam_ForwardsArgument()
    {
        // non-string T1 to disambiguate from the [CallerFilePath] string overload
        var captured = RunCapture(subject => subject.Info("msg {x}", 99));
        captured.Level.Is(LogLevel.Info);
        captured.Data.Has(1);
        captured.Data[0].Is(99);
    }

    [Fact]
    public void Warn_OneParam_ForwardsArgument()
    {
        var captured = RunCapture(subject => subject.Warn("msg {x}", true));
        captured.Level.Is(LogLevel.Warn);
        captured.Data.Has(1);
        captured.Data[0].Is(true);
    }

    [Fact]
    public void Trace_OneParam_ForwardsArgument()
    {
        var captured = RunCapture(subject => subject.Trace("msg {x}", 7));
        captured.Level.Is(LogLevel.Trace);
        captured.Data.Has(1);
        captured.Data[0].Is(7);
    }

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
        var captured = RunCapture(subject =>
            subject.Log(LogLevel.Info, "msg {x1} {x2} {x3} {x4} {x5}", 1, 2, 3, 4, 5)
        );
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

    private static void RunLevelTest(LogLevel expected, Action<ILogSubject> action)
    {
        var captured = RunCapture(action);
        captured.Level.Is(expected);
        captured.Message.Is("msg");
    }

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

    private sealed record CapturedEntry(LogLevel Level, string Message, IReadOnlyList<object?> Data);

    private sealed class CapturingLogger : ILogger
    {
        public List<CapturedEntry> Entries { get; } = new();

        public void Log(
            object subject,
            string file,
            string member,
            int line,
            LogLevel level,
            string message,
            IReadOnlyList<object?> data
        ) => Entries.Add(new CapturedEntry(level, message, data));

        public void Error(
            object subject,
            string file,
            string member,
            int line,
            Exception ex,
            IReadOnlyList<object?> data
        ) => Entries.Add(new CapturedEntry(LogLevel.Error, ex.Message, data));
    }

    private sealed class TestSubject : ILogSubject
    {
        public TestSubject(ILogger logger) => Logger = logger;

        public ILogger Logger { get; }
    }
}
