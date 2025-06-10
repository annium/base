using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Annium.Logging;

namespace Annium.Extensions.Shell.Internal;

/// <summary>
/// Unix/Linux-specific implementation of shell command execution
/// </summary>
internal class UnixShellInstance : ShellInstanceBase
{
    /// <summary>
    /// Initializes a new instance of the Unix shell command executor
    /// </summary>
    /// <param name="cmd">The command and arguments to execute</param>
    /// <param name="logger">The logger instance for shell operations</param>
    public UnixShellInstance(IReadOnlyList<string> cmd, ILogger logger)
        : base(cmd, logger) { }

    /// <summary>
    /// Creates and configures a Unix/Linux process for command execution
    /// </summary>
    /// <returns>A configured Process instance ready for Unix/Linux execution</returns>
    protected override Process GetProcess()
    {
        var process = new Process { EnableRaisingEvents = true };

        process.StartInfo = StartInfo;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.StartInfo.FileName = Cmd[0];
        process.StartInfo.Arguments = string.Join(' ', Cmd.Skip(1));

        this.Trace<string, string>(
            "shell: {fileName} {arguments}",
            process.StartInfo.FileName,
            process.StartInfo.Arguments
        );

        return process;
    }
}
