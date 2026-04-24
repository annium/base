using System;
using System.IO;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
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
    public ShellStdoutTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddShell());
    }

    /// <summary>
    /// A ~100 KB stdout payload is captured in full; no truncation.
    /// Skipped on Windows — this test uses a POSIX shell to generate the payload.
    /// </summary>
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
