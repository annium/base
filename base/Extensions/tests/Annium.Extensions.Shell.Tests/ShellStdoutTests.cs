using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Shell.Tests;

/// <summary>
/// Verifies that <see cref="IShellInstance.RunAsync(System.Threading.CancellationToken)"/> returns
/// the complete stdout of the spawned process even when the output is large enough to fill the
/// OS pipe buffer. Guards against the pre-T3 regression where the process-exit TCS could complete
/// before the async pipe-drain loop finished, truncating captured output.
/// </summary>
public class ShellStdoutTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShellStdoutTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public ShellStdoutTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddShell());
    }

    /// <summary>
    /// A command that cannot start fails the caller rather than half-succeeding, whether it is awaited or
    /// merely started. The started form has no using of its own, so nothing else would release what it
    /// created.
    /// Skipped on Windows - this test drives a POSIX shell.
    /// </summary>
    [Fact]
    public void Start_UnstartableCommand_Throws()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange
        var shell = Get<IShell>();

        // act & assert
        Wrap.It(() => shell.Cmd("no-such-binary-here").Start(TestContext.Current.CancellationToken))
            .Throws<Exception>();
    }

    /// <summary>
    /// The same holds for a command started rather than awaited: it reports cancellation rather than
    /// failing on the streams of a process that was never started.
    /// Skipped on Windows - this test drives a POSIX shell.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Start_TokenAlreadyCancelled_ReportsCancellation()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange
        var shell = Get<IShell>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // act & assert
        Wrap.It(() => shell.Cmd("sh", "-c", "echo never").Start(cts.Token)).Throws<OperationCanceledException>();
    }

    /// <summary>
    /// A command asked for with a token already cancelled does not run at all. Registering the kill before
    /// starting meant the kill fired against a process that did not exist yet, was swallowed, and the
    /// command then ran to completion while the caller was told it had been cancelled.
    /// Skipped on Windows - this test drives a POSIX shell.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_TokenAlreadyCancelled_DoesNotRunTheCommand()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange - a command whose having run is visible afterwards
        var marker = Path.Combine(Path.GetTempPath(), $"annium-shell-{Guid.NewGuid():N}");
        var shell = Get<IShell>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        try
        {
            // act
            await Wrap.It(async () => await shell.Cmd("sh", "-c", $"touch {marker}").RunAsync(cts.Token))
                .ThrowsAsync<OperationCanceledException>();

            // assert - and it really did not run, rather than running unwatched
            await Task.Delay(TimeSpan.FromMilliseconds(300), TestContext.Current.CancellationToken);
            File.Exists(marker).IsFalse("a command reported as cancelled must not have run");
        }
        finally
        {
            File.Delete(marker);
        }
    }

    /// <summary>
    /// Starting a command that cannot start, with a token already cancelled, fails and leaves nothing
    /// behind. The cancellation callback runs synchronously in that case, so the exit handler is already
    /// waiting on the setup that is about to throw.
    /// Skipped on Windows - this test drives a POSIX shell.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_CancelledTokenAndUnstartableCommand_DoesNotStrandWork()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange
        var shell = Get<IShell>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // act & assert - it fails, as it must
        await Wrap.It(async () => await shell.Cmd("no-such-binary-here").RunAsync(cts.Token)).ThrowsAsync<Exception>();

        // and the shell still works afterwards, so nothing is holding it up
        var result = await shell.Cmd("sh", "-c", "echo after").RunAsync(TestContext.Current.CancellationToken);
        result.Output.Trim().Is("after");
    }

    /// <summary>
    /// A command short enough to exit before the caller has finished attaching to its output still
    /// reports it. The exit handler releases the process as soon as it fires, so a process that ends
    /// during setup could be gone by the time the streams were read.
    ///
    /// This is a best-effort net, not a proof: removing the fix does not make this test fail on its own,
    /// even with these concurrent runs. The window needs the machine busy enough to delay the thread doing
    /// the attaching, which in practice means the whole suite running at once - that is where the original
    /// failure showed up, two runs in five. Kept because it costs a second and is the shape that would
    /// catch a regression there.
    /// Skipped on Windows - this test drives a POSIX shell.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_CommandExitsImmediately_IsStillCaptured()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange
        var shell = Get<IShell>();

        // act - run in parallel rather than in sequence: the window this closes needs the thread that
        // attaches to the output to be delayed, which only happens when something else wants the CPU
        var runs = Enumerable
            .Range(0, 8)
            .Select(worker =>
                Task.Run(
                    async () =>
                    {
                        for (var i = 0; i < 10; i++)
                        {
                            var result = await shell
                                .Cmd("sh", "-c", "echo done")
                                .RunAsync(TestContext.Current.CancellationToken);

                            result.IsSuccess.IsTrue($"worker {worker} run {i} must succeed");
                            result.Output.Trim().Is("done", $"worker {worker} run {i} must report its output");
                        }
                    },
                    TestContext.Current.CancellationToken
                )
            )
            .ToArray();

        // assert
        await Task.WhenAll(runs);
    }

    /// <summary>
    /// A command started rather than awaited still reports its whole output through its result. The output
    /// and error streams are drained internally and deliberately not handed out - two readers on one stream
    /// split the bytes between them.
    /// Skipped on Windows - this test drives a POSIX shell.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Start_ResultCarriesTheWholeOutput()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange
        var shell = Get<IShell>();

        // act - repeated, because a split between two readers would show up as an occasional short read
        for (var i = 0; i < 10; i++)
        {
            var started = shell.Cmd("sh", "-c", "echo hello-from-start").Start(TestContext.Current.CancellationToken);

            // assert
#pragma warning disable VSTHRD003
            var result = await started.Result;
#pragma warning restore VSTHRD003
            result.IsSuccess.IsTrue();
            result.Output.Trim().Is("hello-from-start");
        }
    }

    /// <summary>
    /// What the caller writes to a started command's input reaches it.
    /// Skipped on Windows - this test drives a POSIX shell.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Start_InputReachesTheCommand()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange
        var shell = Get<IShell>();
        var started = shell.Cmd("cat").Start(TestContext.Current.CancellationToken);

        // act
        await started.Input.WriteLineAsync("through stdin");
        started.Input.Close();

        // assert
#pragma warning disable VSTHRD003
        var result = await started.Result;
#pragma warning restore VSTHRD003
        result.Output.Trim().Is("through stdin");
    }

    /// <summary>
    /// A ~100 KB stdout payload is captured in full; no truncation.
    /// Skipped on Windows — this test uses a POSIX shell to generate the payload.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task RunAsync_LargeStdout_NotTruncated()
    {
        if (OperatingSystem.IsWindows())
            return;

        // arrange
        const int byteCount = 100_000;
        var scriptPath = Path.Combine(Path.GetTempPath(), $"annium-shell-test-{Guid.NewGuid():N}.sh");
        await File.WriteAllTextAsync(
            scriptPath,
            $"#!/bin/sh\nhead -c {byteCount} /dev/zero | tr '\\0' 'a'\n",
            TestContext.Current.CancellationToken
        );
        File.SetUnixFileMode(scriptPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

        try
        {
            var shell = Get<IShell>();

            // act
            var result = await shell.Cmd($"sh {scriptPath}").RunAsync(TimeSpan.FromSeconds(10));

            // assert
            result.IsSuccess.IsTrue();
            result.Output.Length.Is(byteCount);
        }
        finally
        {
            File.Delete(scriptPath);
        }
    }
}
