using System.Collections.Generic;
using System.Diagnostics;
using Annium.Logging;

namespace Annium.Extensions.Shell.Internal;

/// <summary>
/// Windows-specific implementation of shell command execution using cmd.exe
/// </summary>
internal class WindowsShellInstance : ShellInstanceBase
{
    /// <summary>
    /// Initializes a new instance of the Windows shell command executor
    /// </summary>
    /// <param name="cmd">The command and arguments to execute</param>
    /// <param name="logger">The logger instance for shell operations</param>
    public WindowsShellInstance(IReadOnlyList<string> cmd, ILogger logger)
        : base(cmd, logger) { }

    /// <summary>
    /// Creates and configures a Windows process for command execution using cmd.exe
    /// </summary>
    /// <returns>A configured Process instance ready for Windows execution</returns>
    protected override Process GetProcess()
    {
        var process = new Process { EnableRaisingEvents = true };

        process.StartInfo = StartInfo;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = $"/C {string.Join(' ', Cmd)}";

        this.Trace<string, string>(
            "shell: {fileName} {arguments}",
            process.StartInfo.FileName,
            process.StartInfo.Arguments
        );

        return process;
    }
}
