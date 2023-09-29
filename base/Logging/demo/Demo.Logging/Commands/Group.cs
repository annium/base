using Annium.Extensions.Arguments;

namespace Demo.Logging.Commands;

internal class Group : Annium.Extensions.Arguments.Group, ICommandDescriptor
{
    public static string Id => "";
    public static string Description => "log toolkit";

    public Group()
    {
        Add<ConsoleCommand>();
        Add<InMemoryCommand>();
        Add<SeqCommand>();
    }
}