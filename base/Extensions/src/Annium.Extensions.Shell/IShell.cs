namespace Annium.Extensions.Shell;

public interface IShell
{
    IShellInstance Cmd(string command);
}
