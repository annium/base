using System.Collections.Generic;
using System.Diagnostics;
using Annium.Logging;

namespace Annium.Extensions.Shell.Internal;

internal class WindowsShellInstance : ShellInstanceBase
{
    public WindowsShellInstance(IReadOnlyList<string> cmd, ILogger logger)
        : base(cmd, logger) { }

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
