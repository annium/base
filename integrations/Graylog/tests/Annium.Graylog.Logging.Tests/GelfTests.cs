using System;
using System.Collections.Generic;
using Annium.Graylog.Logging.Internal;
using Annium.Logging;
using Annium.Logging.Shared;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Graylog.Logging.Tests;

/// <summary>
/// Unit tests for the GELF formatter that converts log messages into Graylog Extended Log Format dictionaries.
/// </summary>
public class GelfTests
{
    /// <summary>The Unix-epoch instant used as the default timestamp for synthetic test messages.</summary>
    private static readonly Instant _epoch = Instant.FromUnixTimeMilliseconds(0);

    // ── helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a synthetic <see cref="LogMessage{TContext}"/> for the GELF formatter under test,
    /// with sensible defaults for every field so each test overrides only what it asserts on.
    /// </summary>
    /// <param name="subjectType">Logging subject type name.</param>
    /// <param name="subjectId">Logging subject identifier.</param>
    /// <param name="level">Log level.</param>
    /// <param name="threadId">Originating thread id.</param>
    /// <param name="message">Rendered message text (also reused as the message template).</param>
    /// <param name="exception">Optional exception attached to the message.</param>
    /// <param name="data">Optional structured data entries.</param>
    /// <param name="type">Source type name where the log originated.</param>
    /// <param name="member">Source member name where the log originated.</param>
    /// <param name="line">Source line number (0 omits the location segment).</param>
    /// <param name="instant">Timestamp; defaults to <see cref="_epoch"/> when left unset.</param>
    /// <param name="context">Optional logging context; defaults to a fresh instance.</param>
    /// <returns>A fully-populated log message ready to feed to the formatter.</returns>
    private static LogMessage<TestContext> MakeMessage(
        string subjectType = "SubjectType",
        string subjectId = "subject-1",
        LogLevel level = LogLevel.Info,
        int threadId = 1,
        string message = "hello",
        Exception? exception = null,
        IReadOnlyDictionary<string, object?>? data = null,
        string type = "SourceType",
        string member = "DoWork",
        int line = 42,
        Instant instant = default,
        TestContext? context = null
    ) =>
        new LogMessage<TestContext>(
            context ?? new TestContext(),
            instant == default ? _epoch : instant,
            subjectType,
            subjectId,
            level,
            threadId,
            message,
            exception,
            message,
            data ?? new Dictionary<string, object?>(),
            type,
            member,
            line
        );

    /// <summary>Runs the GELF formatter for <paramref name="project"/> over <paramref name="msg"/> and returns the resulting field dictionary.</summary>
    /// <param name="msg">The log message to format.</param>
    /// <param name="project">The Graylog project (GELF host) name.</param>
    /// <returns>The formatted GELF field dictionary.</returns>
    private static IReadOnlyDictionary<string, object?> Format(
        LogMessage<TestContext> msg,
        string project = "my-project"
    ) => Gelf<TestContext>.CreateFormat(project)(msg);

    // ── level → syslog number ────────────────────────────────────────────────

    /// <summary>Tests that Trace maps to syslog level 7.</summary>
    [Fact]
    public void CreateFormat_LevelTrace_MapsToSyslog7()
    {
        var result = Format(MakeMessage(level: LogLevel.Trace));
        result["level"].Is(7);
    }

    /// <summary>Tests that Debug maps to syslog level 6.</summary>
    [Fact]
    public void CreateFormat_LevelDebug_MapsToSyslog6()
    {
        var result = Format(MakeMessage(level: LogLevel.Debug));
        result["level"].Is(6);
    }

    /// <summary>Tests that Info maps to syslog level 5.</summary>
    [Fact]
    public void CreateFormat_LevelInfo_MapsToSyslog5()
    {
        var result = Format(MakeMessage(level: LogLevel.Info));
        result["level"].Is(5);
    }

    /// <summary>Tests that Warn maps to syslog level 4.</summary>
    [Fact]
    public void CreateFormat_LevelWarn_MapsToSyslog4()
    {
        var result = Format(MakeMessage(level: LogLevel.Warn));
        result["level"].Is(4);
    }

    /// <summary>Tests that Error maps to syslog level 3.</summary>
    [Fact]
    public void CreateFormat_LevelError_MapsToSyslog3()
    {
        var result = Format(MakeMessage(level: LogLevel.Error));
        result["level"].Is(3);
    }

    /// <summary>Tests that an unknown level (None) falls back to syslog level 3.</summary>
    [Fact]
    public void CreateFormat_LevelUnknown_FallsBackToSyslog3()
    {
        var result = Format(MakeMessage(level: LogLevel.None));
        result["level"].Is(3);
    }

    // ── level → text ─────────────────────────────────────────────────────────

    /// <summary>Tests that known levels produce their name as _log_level text and None produces "None".</summary>
    [Fact]
    public void CreateFormat_LogLevelText_MapsCorrectly()
    {
        Format(MakeMessage(level: LogLevel.Trace))["_log_level"].Is("Trace");
        Format(MakeMessage(level: LogLevel.Debug))["_log_level"].Is("Debug");
        Format(MakeMessage(level: LogLevel.Info))["_log_level"].Is("Info");
        Format(MakeMessage(level: LogLevel.Warn))["_log_level"].Is("Warn");
        Format(MakeMessage(level: LogLevel.Error))["_log_level"].Is("Error");
        Format(MakeMessage(level: LogLevel.None))["_log_level"].Is("None");
    }

    // ── fixed scalar fields ──────────────────────────────────────────────────

    /// <summary>Tests that host, timestamp, _thread, _subject, _subject_id, and _source_member are set correctly.</summary>
    [Fact]
    public void CreateFormat_FixedFields_ArePopulatedCorrectly()
    {
        var instant = Instant.FromUnixTimeMilliseconds(1_000_500);
        var msg = MakeMessage(
            subjectType: "MySvc",
            subjectId: "svc-99",
            threadId: 7,
            type: "MyType",
            member: "Execute",
            line: 10,
            instant: instant
        );
        var result = Format(msg, project: "proj");

        result["host"].Is("proj");
        result["_subject"].Is("MySvc");
        result["_subject_id"].Is("svc-99");
        result["_thread"].Is(7);
        result["_source_member"].Is("Execute:10");
        // timestamp = 1_000_500 / 1000 as decimal
        result["timestamp"].Is(1_000_500m / 1000m);
    }

    // ── BuildMessage ─────────────────────────────────────────────────────────

    /// <summary>Tests that short_message contains the full formatted prefix including the "at" location segment when line != 0.</summary>
    [Fact]
    public void CreateFormat_BuildMessage_WithLine_IncludesAtSegment()
    {
        var msg = MakeMessage(
            subjectType: "SvcType",
            subjectId: "id-1",
            threadId: 3,
            type: "MyClass",
            member: "Run",
            line: 55,
            message: "test msg"
        );
        var result = Format(msg);
        result["short_message"].Is("[003] SvcType#id-1 at MyClass.Run:55 >> test msg");
    }

    /// <summary>Tests that short_message omits the "at" location segment when line is 0.</summary>
    [Fact]
    public void CreateFormat_BuildMessage_WithoutLine_OmitsAtSegment()
    {
        var msg = MakeMessage(
            subjectType: "SvcType",
            subjectId: "id-2",
            threadId: 5,
            type: "MyClass",
            member: "Run",
            line: 0,
            message: "no-line msg"
        );
        var result = Format(msg);
        result["short_message"].Is("[005] SvcType#id-2 >> no-line msg");
    }

    // ── exception handling ───────────────────────────────────────────────────

    /// <summary>Tests that full_message is present and includes exception message and stack trace when Exception is set.</summary>
    [Fact]
    public void CreateFormat_WithException_AddsFullMessage()
    {
        Exception ex;
        try
        {
            throw new InvalidOperationException("boom");
        }
        catch (Exception caught)
        {
            ex = caught;
        }

        var msg = MakeMessage(
            subjectType: "S",
            subjectId: "1",
            threadId: 1,
            type: "T",
            member: "M",
            line: 1,
            message: "msg",
            exception: ex
        );
        var result = Format(msg);

        result.ContainsKey("full_message").IsTrue();
        var full = result["full_message"] as string;
        full!.Contains(ex.Message).IsTrue();
        full.Contains(ex.StackTrace!).IsTrue();
    }

    /// <summary>Tests that full_message is absent when Exception is null.</summary>
    [Fact]
    public void CreateFormat_WithoutException_OmitsFullMessage()
    {
        var result = Format(MakeMessage(exception: null));
        result.ContainsKey("full_message").IsFalse();
    }

    // ── Data entries ─────────────────────────────────────────────────────────

    /// <summary>Tests that Data entries appear prefixed with underscore and values are stringified.</summary>
    [Fact]
    public void CreateFormat_DataEntries_AppearWithUnderscorePrefix()
    {
        var data = new Dictionary<string, object?> { ["reqId"] = 42, ["traceId"] = "abc" };
        var result = Format(MakeMessage(data: data));
        result["_reqId"].Is("42");
        result["_traceId"].Is("abc");
    }

    /// <summary>Tests that a null Data value produces a null entry (not absent).</summary>
    [Fact]
    public void CreateFormat_NullDataValue_StoresNull()
    {
        var data = new Dictionary<string, object?> { ["nullKey"] = null };
        var result = Format(MakeMessage(data: data));
        result.ContainsKey("_nullKey").IsTrue();
        result["_nullKey"].Is(null);
    }

    // ── Context properties ───────────────────────────────────────────────────

    /// <summary>Tests that a non-null context public property appears as a snake_case prefixed field.</summary>
    [Fact]
    public void CreateFormat_ContextProperty_AppearsAsSnakeCaseField()
    {
        var ctx = new TestContext { AppName = "my-svc" };
        var result = Format(MakeMessage(context: ctx));
        result["_app_name"].Is("my-svc");
    }

    /// <summary>Tests that a null context property value is omitted from the result.</summary>
    [Fact]
    public void CreateFormat_NullContextProperty_IsOmitted()
    {
        var ctx = new TestContext { AppName = null! };
        var result = Format(MakeMessage(context: ctx));
        result.ContainsKey("_app_name").IsFalse();
    }

    /// <summary>
    /// Tests that when a Data key collides with a context property's snake_case field name,
    /// the Data value wins (the Data loop runs first and both use TryAdd).
    /// </summary>
    [Fact]
    public void CreateFormat_DataKeyCollision_DataValueWins()
    {
        var data = new Dictionary<string, object?> { ["app_name"] = "from-data" };
        var ctx = new TestContext { AppName = "from-context" };
        var result = Format(MakeMessage(data: data, context: ctx));
        result["_app_name"].Is("from-data");
    }
}

/// <summary>
/// Test logging context with a single public readable property used to exercise snake_case reflection.
/// </summary>
public sealed class TestContext
{
    /// <summary>Gets or initialises the application name.</summary>
    public string AppName { get; init; } = "svc";
}
