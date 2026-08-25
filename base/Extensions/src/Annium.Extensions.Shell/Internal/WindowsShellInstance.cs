using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Annium.Logging;

namespace Annium.Extensions.Shell.Internal;

/// <summary>
/// Windows-specific implementation of shell command execution
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
    /// Creates and configures a Windows process for command execution
    /// </summary>
    /// <remarks>
    /// The target executable is launched directly rather than through <c>cmd.exe /C</c>: routing the whole
    /// command line through the shell made every metacharacter in an argument — and arguments here are
    /// paths and branch names, i.e. outside input — interpreted by cmd rather than passed to the program.
    /// Shell builtins are not available as a consequence.
    /// </remarks>
    /// <returns>A configured Process instance ready for Windows execution</returns>
    protected override Process GetProcess()
    {
        var process = new Process { EnableRaisingEvents = true };

        process.StartInfo = StartInfo;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.StartInfo.FileName = Cmd[0];
        foreach (var arg in Cmd.Skip(1))
            process.StartInfo.ArgumentList.Add(arg);

        return process;
    }
}
