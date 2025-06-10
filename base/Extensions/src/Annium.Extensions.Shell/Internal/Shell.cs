using System;

namespace Annium.Extensions.Shell.Internal;

/// <summary>
/// Internal implementation of cross-platform shell command execution
/// </summary>
internal class Shell(Func<string[], IShellInstance> getShellInstance) : IShell
{
    /// <summary>
    /// Creates a shell command instance for the specified command
    /// </summary>
    /// <param name="command">The shell command to execute with its arguments</param>
    /// <returns>A configured shell instance for the command</returns>
    /// <exception cref="InvalidOperationException">Thrown when the command is empty</exception>
    public IShellInstance Cmd(string command)
    {
        var args = command.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 0)
            throw new InvalidOperationException("Shell command must be non-empty");

        return getShellInstance(args);
    }
}
