namespace Annium.Extensions.Shell;

/// <summary>
/// Extension methods for configuring shell command instances
/// </summary>
public static class ShellInstanceExtensions
{
    /// <summary>
    /// Sets the working directory for the shell command execution
    /// </summary>
    /// <param name="shell">The shell instance to configure</param>
    /// <param name="directory">The working directory path for command execution</param>
    /// <returns>The shell instance for method chaining</returns>
    public static IShellInstance At(this IShellInstance shell, string directory) =>
        shell.Configure(x => x.WorkingDirectory = directory);
}
