namespace Annium.Extensions.Shell;

/// <summary>
/// Represents the result of a completed shell command execution
/// </summary>
/// <param name="Code">The exit code returned by the shell command</param>
/// <param name="Output">The standard output produced by the command</param>
/// <param name="Error">The standard error output produced by the command</param>
public sealed record ShellResult(int Code, string Output, string Error)
{
    /// <summary>
    /// Gets a value indicating whether the shell command executed successfully
    /// </summary>
    /// <value>True if the exit code is 0, false otherwise</value>
    public bool IsSuccess => Code == 0;
}
