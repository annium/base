namespace Annium.Extensions.Shell;

public static class ShellInstanceExtensions
{
    public static IShellInstance At(this IShellInstance shell, string directory) =>
        shell.Configure(x => x.WorkingDirectory = directory);
}