using System;
using System.Diagnostics;

namespace Annium.Testing.Elements;

public class TestResult
{
    public TestOutcome Outcome { get; set; } = TestOutcome.None;

    public Exception? Failure { get; set; }

    public Stopwatch Watch { get; } = new();

    public DateTimeOffset ExecutionStart { get; set; }

    public DateTimeOffset ExecutionEnd { get; set; }

    public TimeSpan ExecutionDuration { get; set; }
}