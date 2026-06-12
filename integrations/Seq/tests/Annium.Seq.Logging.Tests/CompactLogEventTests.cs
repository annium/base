using System;
using System.Collections.Generic;
using Annium.Logging;
using Annium.Logging.Shared;
using Annium.Seq.Logging.Internal;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Seq.Logging.Tests;

/// <summary>
/// Unit tests for the CLEF formatter that converts log messages into Compact Log Event Format dictionaries.
/// </summary>
public class CompactLogEventTests
{
    /// <summary>A fixed UTC instant used as the default timestamp so CLEF <c>@t</c> formatting is deterministic.</summary>
    private static readonly Instant _fixedInstant = Instant.FromUtc(2020, 1, 2, 3, 4, 5);

    // ── helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a synthetic <see cref="LogMessage{TContext}"/> for the CLEF formatter under test,
    /// with defaults for every field so each test overrides only what it asserts on.
    /// </summary>
    /// <param name="subjectType">Logging subject type name.</param>
    /// <param name="subjectId">Logging subject identifier.</param>
    /// <param name="level">Log level.</param>
    /// <param name="threadId">Originating thread id.</param>
    /// <param name="message">Rendered message text.</param>
    /// <param name="messageTemplate">Optional message template; defaults to <paramref name="message"/>.</param>
    /// <param name="exception">Optional exception attached to the message.</param>
    /// <param name="data">Optional structured data entries.</param>
    /// <param name="type">Source type name where the log originated.</param>
    /// <param name="member">Source member name where the log originated.</param>
    /// <param name="line">Source line number (0 omits the location segment).</param>
    /// <param name="instant">Timestamp; defaults to <see cref="_fixedInstant"/> when left unset.</param>
    /// <param name="context">Optional logging context; defaults to a fresh instance.</param>
    /// <returns>A fully-populated log message ready to feed to the formatter.</returns>
    private static LogMessage<TestContext> MakeMessage(
        string subjectType = "SubjectType",
        string subjectId = "subject-1",
        LogLevel level = LogLevel.Info,
        int threadId = 1,
        string message = "hello",
        string? messageTemplate = null,
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
            instant == default ? _fixedInstant : instant,
            subjectType,
            subjectId,
            level,
            threadId,
            message,
            exception,
            messageTemplate ?? message,
            data ?? new Dictionary<string, object?>(),
            type,
            member,
            line
        );

    /// <summary>Runs the CLEF formatter for <paramref name="project"/> over <paramref name="msg"/> and returns the resulting field dictionary.</summary>
    /// <param name="msg">The log message to format.</param>
    /// <param name="project">The Seq project name emitted as the CLEF <c>@p</c> field.</param>
    /// <returns>The formatted CLEF field dictionary.</returns>
    private static IReadOnlyDictionary<string, string?> Format(
        LogMessage<TestContext> msg,
        string project = "my-project"
    ) => CompactLogEvent<TestContext>.CreateFormat(project)(msg);

    // ── @t timestamp ─────────────────────────────────────────────────────────

    /// <summary>Tests that @t is formatted as an exact CLEF timestamp string for a known UTC Instant.</summary>
    [Fact]
    public void CreateFormat_Timestamp_FormatsAsClefString()
    {
        var result = Format(MakeMessage(instant: _fixedInstant));
        result["@t"].Is("2020-01-02T03:04:05.000Z");
    }

    /// <summary>Tests that milliseconds are included in the @t timestamp when the Instant has a sub-second component.</summary>
    [Fact]
    public void CreateFormat_Timestamp_IncludesMilliseconds()
    {
        var instant = Instant.FromUtc(2021, 6, 15, 12, 30, 45).Plus(Duration.FromMilliseconds(123));
        var result = Format(MakeMessage(instant: instant));
        result["@t"].Is("2021-06-15T12:30:45.123Z");
    }

    // ── @l level ─────────────────────────────────────────────────────────────

    /// <summary>Tests that @l contains the string representation of the log level.</summary>
    [Fact]
    public void CreateFormat_Level_IsLevelToString()
    {
        Format(MakeMessage(level: LogLevel.Trace))["@l"].Is("Trace");
        Format(MakeMessage(level: LogLevel.Debug))["@l"].Is("Debug");
        Format(MakeMessage(level: LogLevel.Info))["@l"].Is("Info");
        Format(MakeMessage(level: LogLevel.Warn))["@l"].Is("Warn");
        Format(MakeMessage(level: LogLevel.Error))["@l"].Is("Error");
        Format(MakeMessage(level: LogLevel.None))["@l"].Is("None");
    }

    // ── @x exception ─────────────────────────────────────────────────────────

    /// <summary>Tests that @x is present and contains both exception message and stack trace when Exception is set.</summary>
    [Fact]
    public void CreateFormat_WithException_AddsAtX()
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

        var result = Format(MakeMessage(exception: ex));

        result.ContainsKey("@x").IsTrue();
        var xValue = result["@x"];
        xValue!.Contains(ex.Message).IsTrue();
        xValue.Contains(ex.StackTrace!).IsTrue();
    }

    /// <summary>Tests that @x is absent when Exception is null.</summary>
    [Fact]
    public void CreateFormat_WithoutException_OmitsAtX()
    {
        var result = Format(MakeMessage(exception: null));
        result.ContainsKey("@x").IsFalse();
    }

    // ── @p project ───────────────────────────────────────────────────────────

    /// <summary>Tests that @p contains the project string passed to CreateFormat.</summary>
    [Fact]
    public void CreateFormat_Project_AppearsInAtP()
    {
        var result = Format(MakeMessage(), project: "my-service");
        result["@p"].Is("my-service");
    }

    // ── BuildMessagePrefix (@m / @mt) ─────────────────────────────────────────

    /// <summary>Tests that @m includes the full prefix with "at" segment when Line is non-zero.</summary>
    [Fact]
    public void CreateFormat_Message_WithLine_IncludesAtSegmentInPrefix()
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
        result["@m"].Is("[003] SvcType#id-1 at MyClass.Run:55 >> test msg");
    }

    /// <summary>Tests that @m omits the "at" location segment when Line is 0.</summary>
    [Fact]
    public void CreateFormat_Message_WithoutLine_OmitsAtSegmentInPrefix()
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
        result["@m"].Is("[005] SvcType#id-2 >> no-line msg");
    }

    /// <summary>Tests that @mt uses the message template (distinct from the formatted message).</summary>
    [Fact]
    public void CreateFormat_MessageTemplate_ContainsTemplateWithPrefix()
    {
        var msg = MakeMessage(
            subjectType: "S",
            subjectId: "1",
            threadId: 1,
            type: "T",
            member: "M",
            line: 10,
            message: "Hello World",
            messageTemplate: "Hello {Name}"
        );
        var result = Format(msg);
        result["@m"].Is("[001] S#1 at T.M:10 >> Hello World");
        result["@mt"].Is("[001] S#1 at T.M:10 >> Hello {Name}");
    }

    // ── Data entries ──────────────────────────────────────────────────────────

    /// <summary>Tests that Data entries appear with their original keys (no prefix) and values stringified.</summary>
    [Fact]
    public void CreateFormat_DataEntries_AppearWithOriginalKeys()
    {
        var data = new Dictionary<string, object?> { ["reqId"] = 42, ["traceId"] = "abc" };
        var result = Format(MakeMessage(data: data));
        result["reqId"].Is("42");
        result["traceId"].Is("abc");
    }

    /// <summary>Tests that a null Data value produces a null string entry rather than being omitted.</summary>
    [Fact]
    public void CreateFormat_NullDataValue_StoresNull()
    {
        var data = new Dictionary<string, object?> { ["nullKey"] = null };
        var result = Format(MakeMessage(data: data));
        result.ContainsKey("nullKey").IsTrue();
        result["nullKey"].Is(null);
    }

    // ── Context properties ────────────────────────────────────────────────────

    /// <summary>Tests that a non-null context public property appears as a snake_case key.</summary>
    [Fact]
    public void CreateFormat_ContextProperty_AppearsAsSnakeCaseKey()
    {
        var ctx = new TestContext { AppName = "my-svc", RequestId = "req-42" };
        var result = Format(MakeMessage(context: ctx));
        result["app_name"].Is("my-svc");
        result["request_id"].Is("req-42");
    }

    /// <summary>Tests that a null context property value is omitted from the result via TryAdd logic.</summary>
    [Fact]
    public void CreateFormat_NullContextProperty_IsOmitted()
    {
        var ctx = new TestContext { AppName = null!, RequestId = null! };
        var result = Format(MakeMessage(context: ctx));
        result.ContainsKey("app_name").IsFalse();
        result.ContainsKey("request_id").IsFalse();
    }

    /// <summary>Tests that a Data key wins over a context property of the same snake_case name (TryAdd does not overwrite).</summary>
    [Fact]
    public void CreateFormat_DataKeyCollision_DataValueWins()
    {
        var data = new Dictionary<string, object?> { ["app_name"] = "from-data" };
        var ctx = new TestContext { AppName = "from-context" };
        var result = Format(MakeMessage(data: data, context: ctx));
        result["app_name"].Is("from-data");
    }
}

/// <summary>
/// Test logging context with two public readable properties used to exercise snake_case reflection.
/// </summary>
public sealed class TestContext
{
    /// <summary>Gets or initialises the application name.</summary>
    public string AppName { get; init; } = "svc";

    /// <summary>Gets or initialises the request identifier.</summary>
    public string RequestId { get; init; } = "req-0";
}
