using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Shell.Tests;

/// <summary>
/// Verifies what a caller learns from a finished process: its exit code, what it wrote to stderr, and that
/// arguments reach it as given. The suite previously asserted only that a successful run produced output,
/// so a failing command was indistinguishable from a succeeding one.
/// Skipped on Windows — these tests drive POSIX utilities.
/// </summary>
public class ShellResultTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShellResultTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public ShellResultTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddShell());
    }

    /// <summary>
    /// A zero timeout means no limit rather than expiring at once, which is the opposite of what the name
    /// suggests and worth pinning.
    /// Skipped on Windows - these tests drive POSIX utilities.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_ZeroTimeout_RunsWithoutLimit()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange
        var shell = Get<IShell>();

        // act - long enough that an immediate expiry would show
        var result = await shell.Cmd("sh", "-c", "sleep 0.2; echo done").RunAsync(TimeSpan.Zero);

        // assert
        result.IsSuccess.IsTrue();
        result.Output.Trim().Is("done");
    }

    /// <summary>
    /// A failing command reports its own exit code rather than a generic failure.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_FailingCommand_ReportsExitCode()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange
        var shell = Get<IShell>();

        // act
        var result = await shell.Cmd("sh", "-c", "exit 3").RunAsync(TestContext.Current.CancellationToken);

        // assert
        result.Code.Is(3);
    }

    /// <summary>
    /// What a command writes to stderr reaches the caller, and does not leak into stdout.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_CommandWritesToStderr_IsCaptured()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange
        var shell = Get<IShell>();

        // act
        var result = await shell
            .Cmd("sh", "-c", "echo out; echo boom 1>&2")
            .RunAsync(TestContext.Current.CancellationToken);

        // assert
        result.Error.Contains("boom").IsTrue("stderr must reach the caller");
        result.Output.Contains("out").IsTrue("stdout must reach the caller");
        result.Output.Contains("boom").IsFalse("the two streams must stay apart");
    }

    /// <summary>
    /// An argument containing a space arrives as one argument. Building the command line as a single string
    /// cannot express this: it splits on spaces, so a path with a space becomes two arguments.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_ArgumentWithSpace_ArrivesIntact()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange
        var shell = Get<IShell>();
        var path = Path.Combine(Path.GetTempPath(), $"annium shell {Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(path, "content", TestContext.Current.CancellationToken);

        try
        {
            // act - cat receives the path as ONE argument
            var result = await shell.Cmd("cat", path).RunAsync(TestContext.Current.CancellationToken);

            // assert
            result.Code.Is(0);
            result.Output.Contains("content").IsTrue("the file must have been found under its real name");
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// The same path passed through the space-splitting overload does NOT survive — pinning the documented
    /// limitation, so the reason the verbatim overload exists stays visible.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_ArgumentWithSpace_ViaCommandLine_IsSplit()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange
        var shell = Get<IShell>();
        var path = Path.Combine(Path.GetTempPath(), $"annium shell {Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(path, "content", TestContext.Current.CancellationToken);

        try
        {
            // act
            var result = await shell.Cmd($"cat {path}").RunAsync(TestContext.Current.CancellationToken);

            // assert - cat was handed two nonexistent paths instead of one real one
            result.Code.IsNotDefault();
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Cancelling stops the process instead of waiting for it to finish on its own.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_Canceled_StopsTheProcess()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange - a sleep far longer than the test would tolerate
        var shell = Get<IShell>();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        // act
        var run = shell.Cmd("sleep", "60").RunAsync(cts.Token);
        await cts.CancelAsync();

        // assert - the wait ends promptly; without the kill this would hang for a minute
        var completed = await Task.WhenAny(
            run,
            Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken)
        );
        (completed == run).IsTrue("cancellation must stop the process");
    }
}
