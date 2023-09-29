namespace Demo.Core.Cli.Lib;

public interface ICommand
{
    string Name { get; }
    string Description { get; }

    int Execute();
}