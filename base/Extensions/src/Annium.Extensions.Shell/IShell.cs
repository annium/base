namespace Annium.Extensions.Shell;

/// <summary>
/// Interface for executing shell commands
/// </summary>
public interface IShell
{
    /// <summary>
    /// Creates a shell command instance
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <returns>A shell instance for the command</returns>
    IShellInstance Cmd(string command);
}
