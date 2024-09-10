using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Annium.Logging;

namespace Annium.Extensions.Shell.Internal;

internal class UnixShellInstance : ShellInstanceBase
{
    public UnixShellInstance(IReadOnlyList<string> cmd, ILogger logger)
        : base(cmd, logger) { }

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
