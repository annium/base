namespace Annium.Extensions.Shell;

public sealed record ShellResult(int Code, string Output, string Error)
{
    public bool IsSuccess => Code == 0;
}
