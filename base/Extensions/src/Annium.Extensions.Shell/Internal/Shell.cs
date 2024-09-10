using System;

namespace Annium.Extensions.Shell.Internal;

internal class Shell(Func<string[], IShellInstance> getShellInstance) : IShell
{
    public IShellInstance Cmd(string command)
    {
        var args = command.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 0)
            throw new InvalidOperationException("Shell command must be non-empty");

        return getShellInstance(args);
    }
}
