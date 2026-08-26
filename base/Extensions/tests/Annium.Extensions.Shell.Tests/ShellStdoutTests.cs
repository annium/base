using System;
using System.IO;
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
    /// A command short enough to exit before the caller has finished attaching to its output still
    /// reports it. The exit handler releases the process as soon as it fires, so a process that ends
    /// during setup could be gone by the time the streams were read.
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

        // act & assert - repeated, because the window this closes is a race
        for (var i = 0; i < 50; i++)
        {
            var result = await shell.Cmd("sh", "-c", "echo done").RunAsync(TestContext.Current.CancellationToken);

            result.IsSuccess.IsTrue($"run {i} must succeed");
            result.Output.Trim().Is("done", $"run {i} must report its output");
        }
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
