namespace Annium.Extensions.Shell;

public interface IShell
{
    IShellInstance Cmd(params string[] command);
}