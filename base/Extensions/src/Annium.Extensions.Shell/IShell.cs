namespace Annium.Extensions.Shell;

/// <summary>
/// Interface for executing shell commands
/// </summary>
public interface IShell
{
    /// <summary>
    /// Creates a shell command instance from a whole command line, split on spaces.
    /// </summary>
    /// <remarks>
    /// Splitting on spaces means an argument that itself contains a space — any such path — cannot be
    /// expressed this way; it arrives at the process as two arguments. Use <see cref="Cmd(string[])"/> for
    /// anything built from paths or other outside input.
    /// </remarks>
    /// <param name="command">The command to execute</param>
    /// <returns>A shell instance for the command</returns>
    IShellInstance Cmd(string command);

    /// <summary>
    /// Creates a shell command instance from arguments passed verbatim: the executable first, then one
    /// argument per element, each reaching the process exactly as given.
    /// </summary>
    /// <param name="args">The executable and its arguments</param>
    /// <returns>A shell instance for the command</returns>
    IShellInstance Cmd(params string[] args);
}
