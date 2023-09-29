using System;
using Demo.Core.Cli.Lib;

namespace Demo.Core.Cli.PluginHello;

internal class HelloCommand : ICommand
{
    public string Name => "hello";
    public string Description => "Displays hello message.";

    public int Execute()
    {
        Console.WriteLine("Hello!");

        return 0;
    }
}